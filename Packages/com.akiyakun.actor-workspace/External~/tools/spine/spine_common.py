"""Spine のコマンドラインを使うツールの共通処理。

Spine のデータの原本は、プロジェクトごとに .spine か、Spine から書き出した JSON(Nonessential data あり)にする。
JSON を原本にするときは、.spine は原本から作り直せる作業用のファイルとして扱う。

- spine_to_json.py: .spine から原本の JSON を作る(JSON を原本にするプロジェクト用)
- json_to_spine.py: 原本の JSON から作業用の .spine を作る(JSON を原本にするプロジェクト用)
- spine_actor_export_to_unity.py: 原本(JSON か .spine)から Unity 用のデータを書き出す
"""

from __future__ import annotations

import json
import os
import subprocess
import sys
from pathlib import Path

# 使う Spine エディタのバージョン(spine-unity 4.2 に合わせる)
SPINE_VERSION = "4.2.xx"

# 原本と同じフォルダに置く、原本ではない JSON
OVERRIDE_EXPORT_SETTING_FILENAME = "override_export_setting.json"
EXTRA_DATA_FILENAME = "extra_data.json"
NOT_ORIGINAL_JSON_FILENAMES = {OVERRIDE_EXPORT_SETTING_FILENAME, EXTRA_DATA_FILENAME}

# 原本の JSON の書き出しの設定
# MEMO: nonessential を ON にしないと、ボーンの色・アイコン、メッシュの辺などが取り込み直したときに消える
ORIGINAL_EXPORT_SETTING = {
    "class": "export-json",
    "extension": ".json",
    "format": "JSON",
    "prettyPrint": True,
    "nonessential": True,
    "cleanUp": False,
    "packAtlas": None,
    "warnings": True,
}

# 原本を Unity 用に書き出すときに、原本と同じフォルダに作る一時的な .spine
TEMP_SPINE_FILENAME = "_export_temp.spine"

# tools フォルダ(このファイルの 1 つ上)
TOOLS_DIR = Path(__file__).resolve().parent.parent


class SpineToolError(Exception):
    pass


def setup_console() -> None:
    # MEMO: Windows のコンソールの文字コード(cp932)のままだと、Spine の出力の日本語のパスで落ちることがある
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
        sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def find_spine_command(specified: str | None) -> Path:
    candidates = []
    if specified:
        candidates.append(Path(specified))
    if os.environ.get("SPINE_PATH"):
        candidates.append(Path(os.environ["SPINE_PATH"]))
    if sys.platform == "win32":
        # MEMO: Spine.exe はコンソールに出力しないので、Spine.com を使う
        candidates.append(Path(os.environ.get("ProgramFiles", "C:/Program Files")) / "Spine" / "Spine.com")
    elif sys.platform == "darwin":
        candidates.append(Path("/Applications/Spine.app/Contents/MacOS/Spine"))
    else:
        candidates.append(Path("/usr/local/bin/spine"))

    for candidate in candidates:
        # Windows で Spine.exe を指定されたときは、同じ場所の Spine.com を使う
        if candidate.suffix.lower() == ".exe" and candidate.with_suffix(".com").is_file():
            candidate = candidate.with_suffix(".com")
        if candidate.is_file():
            return candidate
    raise SpineToolError(f"Spine が見つかりません。--spine_path か環境変数 SPINE_PATH で指定してください。探した場所: {[str(c) for c in candidates]}")


def run_spine(spine: Path, args: list[str]) -> str:
    command = [str(spine), "--update", SPINE_VERSION, *args]
    print("> " + " ".join(command))
    result = subprocess.run(command, capture_output=True, text=True, encoding="utf-8", errors="replace")
    output = result.stdout.strip()
    print(output)
    # MEMO: Spine は失敗しても終了コードが 0 のことがあるので、出力の「Complete.」も見る
    if result.returncode != 0 or "Complete." not in output:
        print(result.stderr.strip(), file=sys.stderr)
        raise SpineToolError(f"Spine のコマンドが失敗しました。終了コード={result.returncode}")
    return output


def find_original_jsons(folder: Path) -> list[Path]:
    return sorted(p for p in folder.glob("*.json") if p.name not in NOT_ORIGINAL_JSON_FILENAMES)


def find_original_json(folder: Path) -> Path:
    jsons = find_original_jsons(folder)
    if len(jsons) != 1:
        raise SpineToolError(f"原本の JSON が 1 つではありません: {folder}（{[p.name for p in jsons]}）")
    return jsons[0]


def read_json(path: Path) -> dict:
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def write_json(data: dict, path: Path) -> None:
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        f.write("\n")


def without_hash(data: dict) -> dict:
    """skeleton の hash を除いたもの。hash は .spine に取り込むたびに変わるので、比べるときに除く。"""
    data = dict(data)
    data["skeleton"] = {k: v for k, v in data.get("skeleton", {}).items() if k != "hash"}
    return data


def export_original_json(spine: Path, spine_file: Path, work_dir: Path) -> Path:
    """.spine から原本の JSON を、空のフォルダ work_dir に書き出す。ファイル名はスケルトンの名前になる。

    MEMO: JSON の画像のフォルダ(images)は、.spine に設定された相対パスのまま書き出される(書き出し先に合わせて変わらない)。
          なので、作業用のフォルダに書き出してから、.spine と同じフォルダに移してよい
    """
    setting_path = work_dir / "setting" / "original_export_setting.json"
    output_dir = work_dir / "output"
    setting_path.parent.mkdir(parents=True)
    output_dir.mkdir(parents=True)
    write_json(ORIGINAL_EXPORT_SETTING, setting_path)
    run_spine(spine, ["--input", str(spine_file), "--output", str(output_dir), "--export", str(setting_path)])
    exported = sorted(output_dir.glob("*.json"))
    if len(exported) != 1:
        raise SpineToolError(f"書き出した JSON が 1 つではありません（.spine にスケルトンが複数ある？）: {[p.name for p in exported]}")

    # MEMO: 原本はリポジトリに入るので、作業した PC のパスを入れない(ほかの PC では画像も見つからない)
    skeleton = read_json(exported[0]).get("skeleton", {})
    for key in ("images", "audio"):
        if is_absolute_path(skeleton.get(key, "")):
            raise SpineToolError(
                f"{key} のフォルダが絶対パスなので、原本にしませんでした。Spine の {key} のフォルダの設定を .spine からの相対パス（例：./{key}/）にしてください: {spine_file}")
    return exported[0]


def is_absolute_path(value: str) -> bool:
    return value.startswith("/") or (len(value) >= 2 and value[1] == ":")


def import_json_to_spine(spine: Path, json_file: Path, spine_file: Path) -> None:
    """原本の JSON を .spine に取り込む。スケルトンの名前は JSON のファイル名になる。"""
    if spine_file.exists():
        raise SpineToolError(f"取り込み先の .spine がもうあります: {spine_file}")
    run_spine(spine, ["--input", str(json_file), "--output", str(spine_file), "--import"])
