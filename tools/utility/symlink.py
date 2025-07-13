import sys
import os
import argparse


def parse_args():
  parser = argparse.ArgumentParser(description ='desc')

  parser.add_argument('--src', type=str,
    help='シンボリックリンク作成元パス', required=True)
  parser.add_argument('--dest', type=str,
    help='シンボリックリンクの作成先', required=True)
  parser.add_argument('--override', action='store_true',
    help='既に存在する場合でも作成しなおす')

  return parser.parse_args()


if __name__ == "__main__":
  args = parse_args()

  src = os.path.abspath(args.src)
  dest = os.path.abspath(args.dest)

  if args.override and os.path.exists(dest):
    os.remove(dest)

  try:
    # シンボリックリンクの作成
    os.symlink(src, dest)
  except Exception as e:
    print(e)
    sys.exit(1)

  # print("finish.")
  sys.exit(0)
