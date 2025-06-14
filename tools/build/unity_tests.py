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
import sys
import os.path
import subprocess
import argparse


def parse_args():
  parser = argparse.ArgumentParser(description ='desc')

  parser.add_argument('--unity_path', type=str,
    help='Unity application path.', required=True)
  parser.add_argument('--platform', type=str,
    help='Test platform type. [EditMode, PlayMode, Android, iOS, Windows, macOS]', required=True)
  # parser.add_argument('--development', action='store_true')

  return parser.parse_args()


# /Applications/Unity/Hub/Editor/2018.3.7f1/Unity.app/Contents/MacOS/Unity -runTests -projectPath '../../' -testResults 'build/test_results.xml' -testPlatform 'playmode'
if __name__ == '__main__':
  os.chdir('../../')
  args = parse_args()

  # 前処理として1度実行する
  params = [args.unity_path, '-batchmode', '-quit', '-silent-crashes',
    '-projectPath', './',
    '-executeMethod', 'Shared.Editor.Build.CommandLineBuild.Build2',
    '-logFile', './build/unity_tests_build.log'];
  ret2 = subprocess.run(params, stdout = subprocess.PIPE, stderr = subprocess.PIPE)
  print('unity():', params)


  # MEMO: -- で始まるコマンドは CommandLineBuild.cs で独自に作成したコマンドです
  params = [args.unity_path, '-runTests', '-batchmode',# '-nographics',
    '-projectPath', './',
    '-testResults', 'build/test_results.xml',
    # '-executeMethod', 'SharedEditor.CommandLineBuild.Build2',
    '-testPlatform', args.platform];

  # if args.development:
  #   params.append('--development')

  print('unity():', params)
  # print(args.unity_path)

  ret = subprocess.run(params, stdout = subprocess.PIPE, stderr = subprocess.PIPE)
  if ret.returncode != 0:
    print(sys.stdout.buffer.write(ret.stdout))
    print(sys.stdout.buffer.write(ret.stderr))
    print('\nerror.')
    sys.exit(ret.returncode)

  print(sys.stdout.buffer.write(ret.stdout))
  print(sys.stdout.buffer.write(ret.stderr))

  print('finish.')

