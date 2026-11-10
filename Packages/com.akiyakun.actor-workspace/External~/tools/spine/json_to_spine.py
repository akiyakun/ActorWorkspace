"""原本の JSON から作業用の .spine を作る。

Spine で作業を始めるときに使う。原本と同じフォルダに「原本の名前.spine」を作る。
もう .spine があるときは、作業中のものを消さないように、作らずにエラーにする。

使い方(tools フォルダで):
  python spine/json_to_spine.py --inputs <原本の JSON か、原本のあるフォルダ> ...
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

from spine_common import (NOT_ORIGINAL_JSON_FILENAMES, SpineToolError, find_original_json, find_spine_command,
                          import_json_to_spine, setup_console)


def main() -> int:
    setup_console()
    parser = argparse.ArgumentParser(description="原本の JSON から作業用の .spine を作る")
    parser.add_argument("--spine_path", help="Spine の実行ファイル。既定: 環境変数 SPINE_PATH か、標準のインストール先")
    parser.add_argument("--inputs", nargs="+", required=True, help="原本の JSON か、原本のあるフォルダ")
    args = parser.parse_args()

    try:
        spine = find_spine_command(args.spine_path)
        for input_path in args.inputs:
            path = Path(input_path).resolve()
            if path.is_file() and (path.suffix != ".json" or path.name in NOT_ORIGINAL_JSON_FILENAMES):
                raise SpineToolError(f"原本の JSON か、原本のあるフォルダを渡してください: {path}")
            json_file = path if path.is_file() else find_original_json(path)
            spine_file = json_file.with_suffix(".spine")
            if spine_file.exists():
                raise SpineToolError(f".spine がもうあります。作業中のものなら、先に原本の JSON にしてください。使わないなら消してから実行してください: {spine_file}")
            import_json_to_spine(spine, json_file, spine_file)
            print(f"作った: {spine_file}")
    except SpineToolError as e:
        print(f"[エラー] {e}", file=sys.stderr)
        return 1
    print("終わりました。")
    return 0


if __name__ == "__main__":
    sys.exit(main())
