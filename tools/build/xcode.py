#!/usr/bin/env python
#
import sys
import os.path
import subprocess
import argparse
import glob


# プロジェクト情報を表示
def show_proj_info(proj_path):
  ret = subprocess.run(['xcodebuild', '-project', proj_path, '-list'],
    stdout = subprocess.PIPE, stderr = subprocess.PIPE)
  print(sys.stdout.buffer.write(ret.stdout))
  print(sys.stdout.buffer.write(ret.stderr))
  return ret.returncode


# プロジェクトをクリーンアップ
def cleanup(proj_dir):
  # カレントディレクトリにすると -project, -workspace の判断を自動でしてくれる
  old_cwd = os.getcwd()
  os.chdir(proj_dir)

  args = ['xcodebuild', 'clean']
  # print('archive():', args)
  ret = subprocess.run(args, stdout = subprocess.PIPE, stderr = subprocess.PIPE)
  print(sys.stdout.buffer.write(ret.stdout))
  print(sys.stdout.buffer.write(ret.stderr))

  # 作業ディレクトリを戻す
  os.chdir(old_cwd)

  return ret.returncode


# アーカイブファイルを作成
#
# proj_dir        Xcodeプロジェクトがあるフォルダのパス
# scheme          
# output          
#
# example:
# python3 build_ipa.py -proj_dir '../../build/debug/iOS/xcode/' -archive_path '../../build/debug/iOS/app.xcarchive' -ipa_plist 'ipa_development.plist' -out_ipa_dir '../../build/debug/iOS/ipa/'
# python3 build_ipa.py -proj_dir ../../build/debug/iOS/xcode/ -archive_path ../../build/debug/iOS/app.xcarchive -ipa_plist ipa_development.plist -out_ipa_dir ../../build/debug/iOS/ipa/
#
def archive(proj_dir, scheme, output):

  proj_mode = ''
  abs_path = ''

  file_list = glob.glob(proj_dir + "/*.xcworkspace")
  if len(file_list) == 1:
    proj_mode = '-workspace'
  else:
    file_list = glob.glob(proj_dir + "/*.xcodeproj")
    if len(file_list) == 1: proj_mode = '-project'

  if len(file_list) == 1:
    # abs_path = os.path.abspath(file_list[0])
    abs_path = file_list[0]
  else:
    # プロジェクトファイルが見つからなかった
    print('archive(): Can not find of Xcode projects.')
    return 1

  # クリーンアップ
  cleanup(proj_dir)

  # xcodebuild archive -project '../../build/debug/iOS/xcode/Unity-iPhone.xcodeproj' -scheme 'Unity-iPhone' -archivePath '../../build/debug/iOS/app.xcarchive'
  args = ['xcodebuild', 'archive', proj_mode, abs_path, '-scheme', scheme, '-archivePath', output]
  print('archive():', args)

  ret = subprocess.run(args, stdout = subprocess.PIPE, stderr = subprocess.PIPE)
  print(sys.stdout.buffer.write(ret.stdout))
  print(sys.stdout.buffer.write(ret.stderr))

  if ret.returncode != 0:
    print('\nerror.')
    return ret.returncode

  print('archive() finish.')
  return ret.returncode


# アーカイブファイルからiapファイルを作成
#
# archive_path      アーカイブファイルのパス
# plist_file        使用する*.plistファイル
# output            出力先ディレクトリ
#
# example:
#
def export_iap(archive_path, plist_file, output):
  # xcodebuild -exportArchive -archivePath '../../build/iOS/app.xcarchive' -exportPath '../../build/iOS/ipa/' -exportOptionsPlist 'ipa_development.plist'
  args = ['xcodebuild', '-exportArchive', '-archivePath', archive_path,
    '-exportPath', output, '-exportOptionsPlist', plist_file]
  print('export_iap():', args)

  ret = subprocess.run(args, stdout = subprocess.PIPE, stderr = subprocess.PIPE)
  if ret.returncode != 0:
    print(sys.stdout.buffer.write(ret.stdout))
    print(sys.stdout.buffer.write(ret.stderr))
    print('\nerror.')
    return ret.returncode

  print(sys.stdout.buffer.write(ret.stdout))
  print(sys.stdout.buffer.write(ret.stderr))
  print('export_iap() finish.')
  return ret.returncode

