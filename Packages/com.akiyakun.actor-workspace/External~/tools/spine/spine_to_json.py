""".spine から原本の JSON を作る。

デザイナーが作業した .spine を受け取ったときに使う。.spine と同じフォルダに「スケルトンの名前.json」を書き出す。
そのフォルダにもう原本があって名前が違うときは、上書きせずにエラーにする
(古い .spine や、別の原本から作った .spine で上書きしないため)。

使い方(tools フォルダで):
  python spine/spine_to_json.py --inputs <.spine か、.spine のあるフォルダ> ...
"""

from __future__ import annotations

import argparse
import shutil
import sys
import tempfile
from pathlib import Path

from spine_common import (TEMP_SPINE_FILENAME, SpineToolError, export_original_json, find_original_jsons,
                          find_spine_command, read_json, setup_console, without_hash)

# Spine が新しいスケルトンに付ける名前。原本のファイル名(= Unity 用のファイル名)になるので、このままだと分かりにくい
GENERIC_SKELETON_NAMES = {"skeleton"}


def find_spine_file(path: Path) -> Path:
    if path.is_file():
        if path.suffix != ".spine":
            raise SpineToolError(f".spine ではありません: {path}")
        return path
    # MEMO: Unity 用の書き出しの一時的な .spine は、書き出しが止まると残ることがあるので数えない
    spine_files = sorted(p for p in path.glob("*.spine") if p.name != TEMP_SPINE_FILENAME)
    if len(spine_files) != 1:
        raise SpineToolError(f".spine が 1 つではありません: {path}（{[p.name for p in spine_files]}）")
    return spine_files[0]


def save_original(spine: Path, spine_file: Path) -> None:
    folder = spine_file.parent
    with tempfile.TemporaryDirectory() as work_dir:
        exported = export_original_json(spine, spine_file, Path(work_dir))
        destination = folder / exported.name

        others = [p for p in find_original_jsons(folder) if p.name != exported.name]
        if others:
            raise SpineToolError(
                f"このフォルダの原本は {[p.name for p in others]} ですが、.spine のスケルトンの名前は「{exported.stem}」です。"
                f"原本から作った .spine で作業したか確かめてください（原本の名前を変えるときは、先に今の原本を消します）: {spine_file}")

        data = read_json(exported)
        if exported.stem in GENERIC_SKELETON_NAMES:
            print(f"[警告] スケルトンの名前が「{exported.stem}」のままです。原本のファイル名になるので、アクターが分かる名前にしてください")

        # MEMO: hash は .spine に取り込むたびに変わるので、ほかが同じなら原本を書き換えない(差分を出さない)
        if destination.exists() and without_hash(read_json(destination)) == without_hash(data):
            print(f"変わっていない: {destination}")
            return
        shutil.copyfile(exported, destination)
        print(f"書き出した: {destination}")


def main() -> int:
    setup_console()
    parser = argparse.ArgumentParser(description=".spine から原本の JSON を作る")
    parser.add_argument("--spine_path", help="Spine の実行ファイル。既定: 環境変数 SPINE_PATH か、標準のインストール先")
    parser.add_argument("--inputs", nargs="+", required=True, help=".spine か、.spine のあるフォルダ")
    args = parser.parse_args()

    try:
        spine = find_spine_command(args.spine_path)
        for input_path in args.inputs:
            save_original(spine, find_spine_file(Path(input_path).resolve()))
    except SpineToolError as e:
        print(f"[エラー] {e}", file=sys.stderr)
        return 1
    print("終わりました。")
    return 0


if __name__ == "__main__":
    sys.exit(main())
