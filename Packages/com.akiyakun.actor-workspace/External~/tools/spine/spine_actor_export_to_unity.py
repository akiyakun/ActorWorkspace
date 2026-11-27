"""Spine のデータから、Unity 用のデータ(JSON・アトラス)を書き出す。

原本は、書き出した JSON でも .spine でもよい(プロジェクトによって違う)。
原本の JSON は、一時的な .spine に取り込んでから書き出す
(MEMO: JSON から直接アトラスを作ると、連番(シーケンス)の画像が見つからない)。

入力のフォルダ(例: spine/Player/Player_0001@bundle/)
  <スケルトンの名前>.json か .spine  原本(両方あるときは JSON。.spine は作業用のファイルとみなす)
  images/                         画像
  override_export_setting.json    (任意)書き出しの設定のうち、このフォルダだけ変えるもの
  extra_data.json                 (任意)書き出し先にコピーする。externalSpineFilePath でほかの場所の原本を使える
出力: <export_dir>/<カテゴリ(入力の親フォルダの名前)>/<入力のフォルダの名前>/
  フォルダの名前の = から @ の前まではコメント(afl の AutoAddressing がアドレスから除く)。
  コメントを除いた名前が同じフォルダが出力先にあれば、入力のフォルダの名前に変えてから上書きする
  (例: Enemy_5020@bundle → Enemy_5020=ダイエイユー@bundle。Unity のアセットの GUID を変えないため)
  Unity が開いているときは、書き出しの間 Unity の取り込み・保存を止める(afl の ExternalEditLock)。
  Unity が止まったことを確かめられないときは、フォルダの名前を変えずにエラーにする
  Spine は一時フォルダに書き出し、JSON にタイムスタンプを入れてから出力先にコピーする(出力先のファイルは 1 回だけ書く)。
  出力先にある、今回書き出さなかったファイル(.meta や Unity で作ったもの)は消さない

使い方(tools フォルダで):
  python spine/spine_actor_export_to_unity.py --inputs <フォルダ・.spine・原本の JSON> ...

人が使うとき(bin/spine_actor_export_to_unity.bat にドラッグ&ドロップしたときなど)は、ダイアログを出す
(入力が 2 つ以上なら開始の確認、書き出しの間の進み具合、終了・エラーの知らせ。export_dialog.py)。
自動化で使うときは --no_dialog を付けると、ダイアログを出さずにコンソールだけで動く。
tkinter が使えない(ディスプレイが無いなど)ときも、コンソールだけで続ける。
エラーは [エラー] で始まる行を標準エラー出力に出し、終了コード 1 で終わる(最初のエラーで止まる)。
"""

from __future__ import annotations

import argparse
import contextlib
import errno
import shutil
import sys
import tempfile
import time
from datetime import datetime, timezone
from pathlib import Path
from types import SimpleNamespace
from typing import TYPE_CHECKING

from spine_common import (EXTRA_DATA_FILENAME, NOT_ORIGINAL_JSON_FILENAMES, OVERRIDE_EXPORT_SETTING_FILENAME,
                          TEMP_SPINE_FILENAME, TOOLS_DIR, SpineToolError, find_original_json, find_original_jsons,
                          find_spine_command, import_json_to_spine, read_json, run_spine, setup_console,
                          unity_external_edit_lock, write_json)

if TYPE_CHECKING:
    from export_dialog import ExportDialog, ProgressCallback

DEFAULT_EXPORT_DIR = TOOLS_DIR.parent / "Assets" / "AssetBundleData" / "Actor"
# 書き出しの設定。プロジェクト用の設定(bin/spine_export_setting.json)があればそれを使う
PROJECT_EXPORT_SETTING = TOOLS_DIR / "bin" / "spine_export_setting.json"
DEFAULT_EXPORT_SETTING = TOOLS_DIR / "spine" / "default_spine_export_setting.json"
# 書き出したファイルを出力先に置けないときに、やり直す回数と間隔(合計で約 5 秒待つ)
PLACE_RETRY_COUNT = 11
PLACE_RETRY_INTERVAL_SECONDS = 0.5


# by AI
def deep_merge(a: dict, b: dict) -> dict:
    for k, v in b.items():
        if k in a and isinstance(a[k], dict) and isinstance(v, dict):
            deep_merge(a[k], v)
        else:
            a[k] = v
    return a


def add_timestamp_to_exported_json(json_path: Path) -> None:
    timestamp = datetime.now(timezone.utc).isoformat(timespec="seconds").replace("+00:00", "Z")
    original = json_path.read_text(encoding="utf-8")
    newline = "\r\n" if "\r\n" in original else "\n"
    lines = original.splitlines()
    if len(lines) == 0 or lines[0].strip() != "{":
        raise SpineToolError(f"書き出した JSON の先頭が {{ ではありません: {json_path}")
    lines.insert(1, f'"exportTimestamp": "{timestamp}",')
    new_text = newline.join(lines)
    if original.endswith("\n"):
        new_text += newline
    json_path.write_text(new_text, encoding="utf-8", newline="")
    print(f"Timestamp added: {json_path}")


def find_spine_files(folder: Path) -> list[Path]:
    # MEMO: 前の版と同じく、下のフォルダの .spine も探す。書き出しの一時的な .spine は数えない
    return sorted(p for p in folder.rglob("*.spine") if p.name != TEMP_SPINE_FILENAME)


def find_setting_file(folder: Path, filename: str) -> Path | None:
    # MEMO: 前の版と同じく、下のフォルダにあるものも使う(フォルダの直下を優先する)
    if (folder / filename).is_file():
        return folder / filename
    found = sorted(folder.rglob(filename))
    if len(found) > 1:
        raise SpineToolError(f"{filename} が 1 つではありません: {folder}（{[p.relative_to(folder).as_posix() for p in found]}）")
    return found[0] if found else None


def find_source(input_path: Path, folder: Path) -> Path:
    """書き出す元を決める。

    1. 渡された .spine・原本の JSON
    2. フォルダの原本の JSON(JSON を原本にしているプロジェクト)
    3. フォルダの .spine(.spine を原本にしているプロジェクト)
    4. extra_data.json の externalSpineFilePath(ほかの場所の .spine か原本の JSON)
    """
    if input_path.is_file():
        if input_path.suffix == ".spine":
            if find_original_jsons(folder):
                print(f"[警告] このフォルダには原本の JSON がありますが、渡された .spine から書き出します: {input_path}")
            return input_path
        if input_path.suffix == ".json" and input_path.name not in NOT_ORIGINAL_JSON_FILENAMES:
            return input_path
        if input_path.name != EXTRA_DATA_FILENAME:
            raise SpineToolError(f"フォルダ・.spine・原本の JSON・{EXTRA_DATA_FILENAME} のどれかを渡してください: {input_path}")

    jsons = find_original_jsons(folder)
    spine_files = find_spine_files(folder)
    if jsons:
        # MEMO: JSON を原本にしているときは、作業用の .spine が同じフォルダにあっても原本を使う
        #       .spine を原本にしているプロジェクトで JSON が紛れていたときに気付けるように、どちらを使うかを出す
        if spine_files:
            print(f"[情報] .spine（{[p.name for p in spine_files]}）もありますが、JSON（{[p.name for p in jsons]}）を原本として書き出します")
        return find_original_json(folder)

    if len(spine_files) > 1:
        raise SpineToolError(f".spine が 1 つではありません: {folder}（{[p.name for p in spine_files]}）")
    if spine_files:
        return spine_files[0]

    extra_data_path = find_setting_file(folder, EXTRA_DATA_FILENAME)
    if extra_data_path is not None:
        external = read_json(extra_data_path).get("externalSpineFilePath")
        if external is None:
            raise SpineToolError(f"externalSpineFilePath がありません: {extra_data_path}")
        if not external:
            raise SpineToolError(f"externalSpineFilePath が空です: {extra_data_path}")
        # MEMO: 前の版と同じく、入力のフォルダからの相対パス
        return (folder / external).resolve()

    raise SpineToolError(f"書き出すもの（原本の JSON か .spine）が見つかりません: {folder}")


def create_fileinfo(input_path: Path) -> SimpleNamespace:
    # ファイルを渡されたときは、そのフォルダを入力にする
    folder = input_path.parent if input_path.is_file() else input_path
    source_path = find_source(input_path, folder)
    if not source_path.is_file():
        raise SpineToolError(f"書き出す元が見つかりません: {source_path}")

    return SimpleNamespace(
        folder=folder,
        source_path=source_path,
        category_dir_name=folder.parent.name,
        override_export_setting_file_path=find_setting_file(folder, OVERRIDE_EXPORT_SETTING_FILENAME),
        extra_data_file_path=find_setting_file(folder, EXTRA_DATA_FILENAME),
    )


def remove_folder_comment(name: str) -> str:
    """フォルダの名前から、= から @ の前までのコメントを除く(afl の AutoAddressing の RemoveCommentPath と同じく最後の = から)。

    例: Enemy_5020=ダイエイユー@bundle → Enemy_5020@bundle
    MEMO: SpineBulkExporter.cs の RemoveFolderComment と同じ決まり
    """
    at_index = name.rfind("@")
    head, tail = (name[:at_index], name[at_index:]) if at_index >= 0 else (name, "")
    equal_index = head.rfind("=")
    if equal_index >= 0:
        head = head[:equal_index]
    return head + tail


def prepare_output_folder(category_dir: Path, folder_name: str, can_rename: bool) -> Path:
    """出力先のフォルダを決める。

    コメントを除いた名前が同じで、名前が違うフォルダが出力先にあれば、入力のフォルダの名前に変える(.meta も一緒に)。
    入力のフォルダにコメントを足した・変えた・消したときも、Unity の同じアセットを上書きするため(GUID を変えない)。
    can_rename: 名前を変えてよいか(Unity が開いていて、取り込み・保存を止めたことを確かめられないときは False)
    """
    output_path = category_dir / folder_name
    if not category_dir.is_dir():
        return output_path

    key = remove_folder_comment(folder_name)
    others = sorted(p for p in category_dir.iterdir()
                    if p.is_dir() and p.name != folder_name and remove_folder_comment(p.name) == key)
    if not others:
        return output_path
    if output_path.exists() or len(others) > 1:
        names = sorted([p.name for p in others] + ([folder_name] if output_path.exists() else []))
        raise SpineToolError(f"コメント(= から @ の前まで)を除いた名前が同じフォルダが、出力先に複数あります。"
                             f"使うものを 1 つ残して、ほかを Unity で消してください: {category_dir}（{names}）")

    old_path = others[0]
    if not can_rename:
        raise SpineToolError(f"出力先のフォルダの名前を {old_path.name} → {folder_name} に変える必要がありますが、"
                             f"Unity が開いていて取り込みを止められなかったので、変えませんでした。"
                             f"Unity のメニュー App/Develop/ActorWorkspace/Spine/Export All Spine To Unity で書き出すか、"
                             f"Unity で名前を変えてから書き出してください")
    old_meta = old_path.with_name(old_path.name + ".meta")
    new_meta = output_path.with_name(output_path.name + ".meta")
    if new_meta.exists():
        raise SpineToolError(f"名前を変えた先の .meta がもうあります: {new_meta}")
    print(f"[情報] 出力先のフォルダの名前を変えます: {old_path.name} → {folder_name}")
    try:
        old_path.rename(output_path)
        if old_meta.is_file():
            old_meta.rename(new_meta)
    except OSError as e:
        raise SpineToolError(f"出力先のフォルダの名前を変えられませんでした(ほかのアプリがファイルを開いているかもしれません。"
                             f"フォルダと .meta の名前がそろっているか確かめてください): {old_path.name} → {folder_name}: {e}") from e
    return output_path


def export(spine: Path, info: SimpleNamespace, export_dir: Path, export_setting: Path, can_rename: bool) -> None:
    # 今回使う設定(フォルダごとの上書きがあればマージする)
    # MEMO: 設定が読めないときに、書き出し先のフォルダの名前を変えたり作ったりしないように、先に読む
    setting = read_json(export_setting)
    if info.override_export_setting_file_path is not None:
        deep_merge(setting, read_json(info.override_export_setting_file_path))

    output_path = prepare_output_folder(export_dir / info.category_dir_name, info.folder.name, can_rename)
    print(f"output_path: {output_path}")
    output_path.mkdir(parents=True, exist_ok=True)

    # MEMO: 一時フォルダを消せなくても(ウイルス対策ソフトが画像を開いているなど)、書き出しは終わっているのでエラーにしない
    #       (TemporaryDirectory の ignore_cleanup_errors は Python 3.10 から。Mac の python3 は 3.9 のことがある)
    work_dir = tempfile.mkdtemp()
    try:
        setting_path = Path(work_dir) / "export_setting.json"
        write_json(setting, setting_path)
        # MEMO: Spine は一時フォルダに書き出し、タイムスタンプを入れてから出力先に置く。
        #       出力先のファイルを書いた直後に書き直すと、Unity を開いているときに書き直せないことがある
        #       (書いたばかりのファイルを Unity がメモリに割り当てて開いているらしく、上書きが Errno 22 で失敗する)
        spine_output = Path(work_dir) / "output"
        spine_output.mkdir()

        if info.extra_data_file_path is not None:
            shutil.copy(info.extra_data_file_path, spine_output / EXTRA_DATA_FILENAME)

        if info.source_path.suffix == ".spine":
            # .spine を原本にしているプロジェクト(前の版と同じ書き出し方)
            spine_file = info.source_path
            temp_spine = None
        else:
            # MEMO: 一時的な .spine は原本と同じフォルダに作る。別のフォルダに作ると、
            #       書き出した JSON の画像のフォルダ(images)がそこからの相対パスになり、PC ごとに変わる
            temp_spine = info.source_path.parent / TEMP_SPINE_FILENAME
            temp_spine.unlink(missing_ok=True)
            spine_file = temp_spine
        try:
            if temp_spine is not None:
                import_json_to_spine(spine, info.source_path, temp_spine)
            run_spine(spine, ["--input", str(spine_file), "--output", str(spine_output), "--export", str(setting_path)])
        finally:
            if temp_spine is not None:
                temp_spine.unlink(missing_ok=True)

        # 書き出した JSON にタイムスタンプを足す
        # MEMO: 書き出したファイルの名前はスケルトンの名前で、.spine のファイル名とは違うことがあるので、書き出した JSON を探す
        written = sorted(p for p in spine_output.glob("*.json") if p.name not in NOT_ORIGINAL_JSON_FILENAMES)
        if len(written) == 1:
            add_timestamp_to_exported_json(written[0])
        else:
            print(f"[警告] 書き出した JSON が 1 つに決まらないので、タイムスタンプを足さなかった: {[p.name for p in written]}")

        place_exported_files(spine_output, output_path)
    finally:
        shutil.rmtree(work_dir, ignore_errors=True)


def place_exported_files(src_dir: Path, output_path: Path) -> None:
    """一時フォルダに書き出したファイルを、出力先に置く(上書きする)。

    出力先にある、今回書き出さなかったファイル(.meta や Unity で作ったもの)はそのまま残す。
    MEMO: JSON は最後に置く(途中で失敗したときに、JSON だけが新しくなって古いアトラス・画像と食い違わないように)
    """
    files = sorted((p for p in src_dir.rglob("*") if p.is_file()),
                   key=lambda p: (p.suffix == ".json" and p.name not in NOT_ORIGINAL_JSON_FILENAMES, str(p)))
    for src in files:
        dst = output_path / src.relative_to(src_dir)
        copy_file_with_retry(src, dst)
        print(f"Placed: {dst}")


def copy_file_with_retry(src: Path, dst: Path) -> None:
    """ファイルをコピーする。ほかのアプリが開いていて書けないときは、少し待ってやり直す。"""
    for i in range(PLACE_RETRY_COUNT):
        try:
            dst.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(src, dst)
            return
        except OSError as e:
            # MEMO: ほかのアプリが開いているときは、PermissionError か Errno 22(メモリに割り当てて開いているとき)になる
            is_busy = isinstance(e, PermissionError) or e.errno == errno.EINVAL
            if not is_busy:
                raise SpineToolError(f"{dst} に書けません: {e}") from e
            if i == PLACE_RETRY_COUNT - 1:
                raise SpineToolError(f"{dst} を書き換えられません（Unity などほかのアプリが開いているかもしれません）: {e}") from e
            time.sleep(PLACE_RETRY_INTERVAL_SECONDS)


def parse_args():
    parser = argparse.ArgumentParser(description="Spine のデータ(原本の JSON か .spine)から Unity 用のデータを書き出す")
    parser.add_argument("--spine_path", type=str, help="Spine の実行ファイル。既定: 環境変数 SPINE_PATH、Unity プロジェクトの .env の SPINE_PATH、標準のインストール先の順に探す")
    parser.add_argument("--inputs", type=str, nargs="+", required=True, help="原本のあるフォルダ、.spine、原本の JSON")
    parser.add_argument("--export_dir", type=str, default=str(DEFAULT_EXPORT_DIR), help="出力先のフォルダ")
    parser.add_argument("--spine_export_setting", type=str, default=None,
                        help="書き出しの設定。既定: bin/spine_export_setting.json があればそれ、無ければ spine/default_spine_export_setting.json")
    parser.add_argument("--no_dialog", action="store_true",
                        help="ダイアログを出さずに、コンソールだけで動く(自動化で使うとき)")
    return parser.parse_args()


def create_dialog() -> ExportDialog | None:
    """ダイアログを作る。tkinter が使えないときは None(コンソールだけで続ける)。"""
    try:
        from export_dialog import ExportDialog
        return ExportDialog()
    except Exception as e:  # noqa: BLE001 tkinter が無い(ImportError)・ディスプレイが無い(TclError)など
        print(f"[警告] ダイアログを出せないので、コンソールだけで続けます: {e}")
        return None


def export_all(args: argparse.Namespace, export_dir: Path, export_setting: Path,
               progress: ProgressCallback | None = None) -> None:
    # 書き出しの設定が読めるかを、書き出し先に手を付ける前に確かめる
    read_json(export_setting)
    spine = find_spine_command(args.spine_path)
    infos = [create_fileinfo(Path(p).resolve()) for p in args.inputs]
    # 書き出しの間、Unity の取り込み・保存を止める(Unity が書き出しの途中のファイルを取り込んだり、
    # 名前を変えたフォルダに保存しようとして落ちたりしないように)
    with unity_external_edit_lock(TOOLS_DIR.parent, "spine_actor_export_to_unity") as lock:
        can_rename = not lock.is_unity_running or lock.is_acknowledged
        for i, info in enumerate(infos):
            print(f"--- {info.source_path}")
            if progress is not None:
                progress(i, len(infos), str(info.source_path))
            export(spine, info, export_dir, export_setting, can_rename)


# 例(tools フォルダで):
# python spine/spine_actor_export_to_unity.py --inputs ../../spine/Player/Player_0001@bundle
def main() -> int:
    setup_console()
    args = parse_args()
    export_dir = Path(args.export_dir).resolve()
    if args.spine_export_setting:
        export_setting = Path(args.spine_export_setting).resolve()
    else:
        export_setting = PROJECT_EXPORT_SETTING if PROJECT_EXPORT_SETTING.is_file() else DEFAULT_EXPORT_SETTING
    print(f"export_dir: {export_dir}")
    print(f"export_setting: {export_setting}")

    dialog = None if args.no_dialog else create_dialog()
    try:
        # 前の版と同じく、入力が 2 つ以上のときだけ開始を確かめる
        if dialog is not None and len(args.inputs) > 1 and not dialog.confirm_start(len(args.inputs)):
            print("取りやめました。")
            return 0
        if dialog is not None:
            dialog.run(lambda progress: export_all(args, export_dir, export_setting, progress))
        else:
            export_all(args, export_dir, export_setting)
        print("Spineのエクスポート処理が終了しました。")
        if dialog is not None:
            dialog.show_done()
        return 0
    except (SpineToolError, OSError) as e:
        # MEMO: ファイルの操作の失敗(ほかのアプリが開いている、消せないなど)も、トレースバックを出さずに知らせる
        message = str(e) if isinstance(e, SpineToolError) else f"ファイルを操作できませんでした: {e}"
        print(f"[エラー] {message}", file=sys.stderr)
        if dialog is not None:
            dialog.show_error(message)
        return 1
    except Exception as e:
        # 思わぬエラーも、ダイアログで知らせてから今までどおり投げる(コンソールにトレースバックが出る)
        if dialog is not None:
            # MEMO: ダイアログを出せなくても、元のエラーを投げる
            with contextlib.suppress(Exception):
                dialog.show_error(f"{type(e).__name__}: {e}")
        raise
    finally:
        if dialog is not None:
            dialog.close()


if __name__ == "__main__":
    sys.exit(main())
