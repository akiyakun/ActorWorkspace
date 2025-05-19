#!/usr/bin/env python
#
import sys
import os.path
import subprocess
import argparse
import xcode


def parse_args():
  parser = argparse.ArgumentParser(description = 'desc')
  parser.add_argument('-proj_dir', type=str, help='Xcode project directory.', required=True)
  parser.add_argument('-archive_path', type=str, help='*.xcarchive file path.', required=True)
  parser.add_argument('-ipa_plist', type=str, help='*.plist file path.', required=True)
  parser.add_argument('-out_ipa_dir', type=str, help='Export .ipa folder.', required=True)
  return parser.parse_args()


# python3 build_ipa.py -proj_dir "../../build/debug/iOS/xcode" -archive_path '../../build/debug/iOS/app.xcarchive' -ipa_plist 'ipa_development.plist' -out_ipa_dir '../../build/debug/iOS/ipa/' > ../../build/debug_ios_ipa.log
if __name__ == '__main__':
  args = parse_args()

  # if args.cleanup is not None:
  #   xcode.cleanup()

  ret = xcode.archive(args.proj_dir, 'Unity-iPhone', args.archive_path)
  if ret != 0: sys.exit(ret)

  ret = xcode.export_iap(args.archive_path, args.ipa_plist, args.out_ipa_dir)
  if ret != 0: sys.exit(ret)

  print('build_iap() finish.')

