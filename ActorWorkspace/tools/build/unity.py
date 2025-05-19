#!/usr/bin/env python
#
# Unityパスの例
#
# macOS
# Unity単体 '/Applications/Unity/Unity.app/Contents/MacOS/Unity'
# UnityHub '/Applications/Unity/Hub/Editor/2018.3.5f1/Unity.app/Contents/MacOS/Unity'
#
# Windows
# 'C:¥Program Files¥Unity¥Editor¥Unity.exe'
#
# Example:
# python3 unity.py --unity_path "/Applications/Unity/Hub/Editor/2018.3.11f1/Unity.app/Contents/MacOS/Unity" --platform iOS --development
#
import sys
import os
import os.path
import shutil
import subprocess
import argparse

# project_path = '../../'
# build_root_path = project_path + 'build/'
# build_root_path = 'build/'
# build_type_path = 'release/'

def parse_args():
  parser = argparse.ArgumentParser(description ='desc')

  parser.add_argument('--unity_path', type=str,
    help='Unity application path.', required=True)
  parser.add_argument('--platform', type=str,
    help='Build platform type. [Android, iOS, Windows, macOS]', required=True)
  parser.add_argument('--development', action='store_true')

  return parser.parse_args()


if __name__ == '__main__':
  os.chdir('../../')
  args = parse_args()

  # ログファイル名
  logFile = './build/unity_' + args.platform
  if args.development:
    logFile += '_debug_build.log'
  else:
    logFile += '_release_build.log'

  # MEMO: -- で始まるコマンドは CommandLineBuild.cs で独自に作成したコマンドです
  params = [args.unity_path, '-batchmode', '-quit', '-silent-crashes',
    '-projectPath', './',
    '-executeMethod', 'Shared.Editor.Build.CommandLineBuild.Build',
    '-logFile', logFile,
    '--platform', args.platform];

  if args.development:
    params.append('--development')

  print('unity():', params)

  ret = subprocess.run(params, stdout = subprocess.PIPE, stderr = subprocess.PIPE)

  if ret.returncode != 0:
    print(sys.stdout.buffer.write(ret.stdout))
    print(sys.stdout.buffer.write(ret.stderr))
    print('error.')
    sys.exit(ret.returncode)

  print(sys.stdout.buffer.write(ret.stdout))
  print(sys.stdout.buffer.write(ret.stderr))

  # MEMO: C#側でExitコードを返すように処理してみました
  # Xcodeプロジェクトエクスポート時のビルドエラーが検出できない(エラーコードが返ってこない)のでファイルチェックをする
  # if args.platform == 'iOS':
  #   if not os.path.exists(location_path + 'Unity-iPhone.xcodeproj'):
  #     print('Export xcode project failed.')
  #     sys.exit(1)

  print('finish.')
