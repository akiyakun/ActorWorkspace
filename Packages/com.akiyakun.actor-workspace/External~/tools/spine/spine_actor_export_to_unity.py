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

使い方(tools フォルダで):
  python spine/spine_actor_export_to_unity.py --inputs <フォルダ・.spine・原本の JSON> ...
"""

from __future__ import annotations

import argparse
import shutil
import sys
import tempfile
from datetime import datetime, timezone
from pathlib import Path
from types import SimpleNamespace

from spine_common import (EXTRA_DATA_FILENAME, NOT_ORIGINAL_JSON_FILENAMES, OVERRIDE_EXPORT_SETTING_FILENAME,
                          TEMP_SPINE_FILENAME, TOOLS_DIR, SpineToolError, find_original_json, find_original_jsons,
                          find_spine_command, import_json_to_spine, read_json, run_spine, setup_console, write_json)

DEFAULT_EXPORT_DIR = TOOLS_DIR.parent / "Assets" / "AssetBundleData" / "Actor"
# 書き出しの設定。プロジェクト用の設定(bin/spine_export_setting.json)があればそれを使う
PROJECT_EXPORT_SETTING = TOOLS_DIR / "bin" / "spine_export_setting.json"
DEFAULT_EXPORT_SETTING = TOOLS_DIR / "spine" / "default_spine_export_setting.json"


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


def export(spine: Path, info: SimpleNamespace, export_dir: Path, export_setting: Path) -> None:
    output_path = export_dir / info.category_dir_name / info.folder.name
    print(f"output_path: {output_path}")
    output_path.mkdir(parents=True, exist_ok=True)

    with tempfile.TemporaryDirectory() as work_dir:
        # 今回使う設定(フォルダごとの上書きがあればマージする)
        setting = read_json(export_setting)
        if info.override_export_setting_file_path is not None:
            deep_merge(setting, read_json(info.override_export_setting_file_path))
        setting_path = Path(work_dir) / "export_setting.json"
        write_json(setting, setting_path)

        if info.extra_data_file_path is not None:
            shutil.copy(info.extra_data_file_path, output_path / EXTRA_DATA_FILENAME)

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
        before = json_mtimes(output_path)
        try:
            if temp_spine is not None:
                import_json_to_spine(spine, info.source_path, temp_spine)
            run_spine(spine, ["--input", str(spine_file), "--output", str(output_path), "--export", str(setting_path)])
        finally:
            if temp_spine is not None:
                temp_spine.unlink(missing_ok=True)

    # 書き出した JSON にタイムスタンプを足す
    # MEMO: 書き出したファイルの名前はスケルトンの名前で、.spine のファイル名とは違うことがあるので、書き出しで変わった JSON を探す
    written = [p for p, mtime in json_mtimes(output_path).items() if before.get(p) != mtime]
    if len(written) == 1:
        add_timestamp_to_exported_json(written[0])
    else:
        print(f"[警告] 書き出した JSON が 1 つに決まらないので、タイムスタンプを足さなかった: {[p.name for p in written]}")


def json_mtimes(folder: Path) -> dict[Path, int]:
    return {p: p.stat().st_mtime_ns for p in folder.glob("*.json") if p.name not in NOT_ORIGINAL_JSON_FILENAMES}


def parse_args():
    parser = argparse.ArgumentParser(description="Spine のデータ(原本の JSON か .spine)から Unity 用のデータを書き出す")
    parser.add_argument("--spine_path", type=str, help="Spine の実行ファイル。既定: 環境変数 SPINE_PATH か、標準のインストール先")
    parser.add_argument("--inputs", type=str, nargs="+", required=True, help="原本のあるフォルダ、.spine、原本の JSON")
    parser.add_argument("--export_dir", type=str, default=str(DEFAULT_EXPORT_DIR), help="出力先のフォルダ")
    parser.add_argument("--spine_export_setting", type=str, default=None,
                        help="書き出しの設定。既定: bin/spine_export_setting.json があればそれ、無ければ spine/default_spine_export_setting.json")
    return parser.parse_args()


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

    try:
        spine = find_spine_command(args.spine_path)
        infos = [create_fileinfo(Path(p).resolve()) for p in args.inputs]
        for info in infos:
            print(f"--- {info.source_path}")
            export(spine, info, export_dir, export_setting)
    except SpineToolError as e:
        print(f"[エラー] {e}", file=sys.stderr)
        return 1
    print("Spineのエクスポート処理が終了しました。")
    return 0


if __name__ == "__main__":
    sys.exit(main())
