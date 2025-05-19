#!/usr/bin/env python
#
import sys
import os
import os.path
import shutil
import argparse

def parse_args():
  parser = argparse.ArgumentParser(description ='desc')

  parser.add_argument('-src', type=str,
    help='', required=True)
  parser.add_argument('-dest', type=str,
    help='', required=True)

  return parser.parse_args()


if __name__ == '__main__':
  args = parse_args()

  if os.path.exists(args.src) == False:
    print("Not found src: " + args.src)
    sys.exit(0)

  # if os.path.exists(args.dest) == False:
  #   print("Not found dest: " + args.dest)
  #   sys.exit(0)

  shutil.copytree(args.src, args.dest)

  print('finish.')
