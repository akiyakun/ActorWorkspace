from lib.core import *
from lib import afl
import sys
import os
import os.path
import argparse
import shutil
import filecmp
import json
from types import SimpleNamespace

import subprocess
import time
import tkinter as tk
from tkinter import messagebox
from tkinter import ttk
from send2trash import send2trash


msgbox_title = "spine_actor_export_to_unity"
export_dir = "../Assets/AssetBundleData/Actor/"
spine_export_settings_file = "./spine/default_spine_export_setting.json"
temp_export_settings_file = "./spine/_temp_export_setting.json"
override_export_setting_filename = "override_export_setting.json"
extra_data_filename = "extra_data.json"


# by AI
def deep_merge(a: dict, b: dict) -> dict:
    for k, v in b.items():
        if k in a and isinstance(a[k], dict) and isinstance(v, dict):
            deep_merge(a[k], v)
        else:
            a[k] = v
    return a

# by AI
def merge_json_files(path_a: str, path_b: str, output_path: str = None):
    with open(path_a, "r", encoding="utf-8") as fa:
        data_a = json.load(fa)
    with open(path_b, "r", encoding="utf-8") as fb:
        data_b = json.load(fb)

    merged = deep_merge(data_a, data_b)  # BでAを上書き

    # 保存先が未指定ならAと同じ場所に上書き保存
    save_path = output_path or path_a
    with open(save_path, "w", encoding="utf-8") as fo:
        json.dump(merged, fo, ensure_ascii=False, indent=2)
    print(f"Saved: {save_path}")


# e.g. spine_dir_path
# /Spine/Player/Player_0001@bundle/
# /Spine/Player/Player_0001@bundle/Player.spine
# /Spine/Player/Player_0001@bundle/extra_data.json
def create_fileinfo(spine_dir_path: str):
  spine_path = None
  category_dir_name = None
  override_export_setting_file_path = None
  extra_data_file_path = None

  # カテゴリ名取得
  # e.g. "Player"
  category_dir_name = os.path.basename(os.path.dirname(spine_dir_path))

  # フォルダ内のファイルを捜査
  for root, dirs, files in os.walk(spine_dir_path):
    for file in files:
      # print(f"Checking file: {file}")
      if file.endswith(".spine"):
        # .spineファイルが存在
        spine_path = os.path.join(root, file)
      elif file == override_export_setting_filename:
        # オーバーライド設定ファイルが存在
        override_export_setting_file_path = os.path.join(root, override_export_setting_filename)
      elif file == extra_data_filename:
        # エクストラデータファイルが存在
        extra_data_file_path = os.path.join(root, extra_data_filename)

  # .spineファイルが無く、エクストラデータファイルのみ存在する場合
  if spine_path is None:
    if extra_data_file_path is not None:
      # エクストラデータファイルを読み込み、externalSpineFilePathを取得して.spineファイルのパスを決定
      with open(extra_data_file_path, "r", encoding="utf-8") as f:
        extra_data = json.load(f)
        if "externalSpineFilePath" in extra_data:
          external_spine_path = extra_data["externalSpineFilePath"]
          if not external_spine_path:
            print_exception(f"externalSpineFilePath フィールドが空です: {spine_dir_path}")
          spine_path = os.path.normpath(os.path.join(spine_dir_path, external_spine_path))
        else:
          print_exception(f"externalSpineFilePath フィールドがありません: {spine_dir_path}")
    else:
      print_exception(f"エクスポート対象ファイルが見つかりません: {spine_dir_path}")

  return SimpleNamespace(
    spine_path = spine_path,
    spine_dir_path = spine_dir_path,
    category_dir_name = category_dir_name,
    override_export_setting_file_path = override_export_setting_file_path,
    extra_data_file_path = extra_data_file_path
  )


def get_spine_filelist(inputs):
  ret = []

  for i in range(len(inputs)):
    # 絶対パスに変換
    path = os.path.abspath(inputs[i])

    # ディレクトリでなくファイルが渡された場合
    if os.path.isdir(path) == False:
      if path.endswith(".spine"):
        # .spint ファイルは入力として受け付ける
        path = os.path.dirname(path)
      elif path.endswith(extra_data_filename):
        # extra_data.json ファイルは入力として受け付ける
        path = os.path.dirname(path)
      else:
        print_exception(f"無効なファイルが渡されました: {path}")

    info = create_fileinfo(path)
    print(info)

    ret.append(info)

  return ret



def parse_args():
  parser = argparse.ArgumentParser(description ='desc')

  parser.add_argument('--spine_path', type=str,
    help='SpineEditorの実行ファイルパス', required=True)
  parser.add_argument('--inputs', type=str, nargs='*',
    help='Spineプロジェクトファイル', required=True)

  parser.add_argument('--export_dir', type=str, default=export_dir,
    help='出力先ディレクトリ', required=False)
  parser.add_argument('--spine_export_setting', type=str, default=spine_export_settings_file,
    help='Spineエクスポート設定ファイル', required=False)

  return parser.parse_args()


# 例:
# python3 -u -m spine.spine_actor_export_to_unity --spine_path /Applications/Spine.app/Contents/MacOS/Spine --inputs Player@bundle
# if __name__ == '__main__':
def main():
  args = parse_args()
  export_dir = args.export_dir
  spine_export_settings_file = os.path.abspath(args.spine_export_setting)

  # Tkinterのルートウィンドウを非表示で生成
  root = tk.Tk()
  # root.withdraw()
  root.title("プログレスバー")

  # 複数のファイルが渡されたときだけ確認メッセージボックスを表示
  if len(args.inputs) > 1:
    if messagebox.askyesno(msgbox_title, "Spineのエクスポート処理を開始します。"):
      print("Start export...")
    else:
      sys.exit(0)

  # time.sleep(1)
  # sys.exit(0)

  # print(sys.argv[0])
  # print(os.path.abspath(sys.argv[0]))
  # print("dirname:")
  # print(os.path.dirname(os.path.abspath(sys.argv[0])))
  # print("dirname2:")
  # print(os.path.dirname(os.path.dirname(os.path.abspath(sys.argv[0]))))

  # カレントディレクトリを設定(tools ディレクトリ)
  os.chdir(os.path.dirname(os.path.dirname(os.path.abspath(sys.argv[0]))))
  current_dir = os.getcwd()
  print(current_dir)


  # フルパスにしておく
  export_dir = os.path.abspath(export_dir)
  print(f"export_dir: {export_dir}")

  # 出力ディレクトリを削除(ごみ箱)
  # if os.path.exists(export_dir):
  #   send2trash(export_dir)

  # spineファイルのリストを取得
  spine_files = get_spine_filelist(args.inputs)

  label1 = tk.Label(root, text="処理中...")
  label1.pack(side="top")
  # progbar = ttk.Progressbar(root, length=len(spine_files), mode="determinate", maximum=1)
  progbar = ttk.Progressbar(root, length=400, mode="determinate", maximum=1)
  progbar.pack()
  root.lift()
  root.attributes("-topmost", True)

  # progbar.configure(value=0)
  # progbar.update()

  # prog_list = list('abcdefghijklmnopqrstuvwxyz')
  # for i in range(len(prog_list)):
  #   time.sleep(0.05)
  #   # プログレスバー表示変更
  #   progbar.configure(value=(i+1)/len(prog_list))
  #   progbar.update()

  for i in range(len(spine_files)):
    info = spine_files[i]

    output_path = os.path.join(export_dir, info.category_dir_name)
    # e.g. /Spine/Player
    output_path = os.path.join(output_path, os.path.basename(info.spine_dir_path))
    # e.g. /Spine/Player/Player_0001@bundle
    print(f"output_path: {output_path}")
    os.makedirs(output_path, exist_ok=True)
    # sys.exit(1)

    # messagebox.showinfo('メッセージ', input_path)

    # プログレスバー表示変更
    label1.configure(text=f"{info.spine_path}")
    progbar.configure(value=i/len(spine_files))
    progbar.update()

    # 今回使用する設定ファイルをコピー
    shutil.copy(spine_export_settings_file, temp_export_settings_file)

    # オーバーライド設定ファイルがあればマージ
    if info.override_export_setting_file_path is not None:
      if os.path.isfile(info.override_export_setting_file_path):
          merge_json_files(temp_export_settings_file, info.override_export_setting_file_path)

    # エクストラデータファイルが存在する場合コピー
    if info.extra_data_file_path is not None:
      shutil.copy(info.extra_data_file_path, os.path.join(output_path, extra_data_filename))


    try:
      # See also: https://ja.esotericsoftware.com/spine-command-line-interface
      result = subprocess.run(
        # 実行したいコマンド（例: ls -l）
        # "/Applications/Spine.app/Contents/MacOS/Spine",
        [
          args.spine_path,
          "--update", "4.2.xx",
          "--input", f"{info.spine_path}",
          "--output", f"{output_path}",
          "--export", temp_export_settings_file
        ],
        capture_output=True,         # 標準出力・標準エラー出力を取得
        text=True,                   # 出力を文字列として取得
        check=True
      )
    except subprocess.CalledProcessError as e:
      messagebox.showerror(msgbox_title, f"エクスポート失敗: {info.spine_dir_path}\n\n{e.stderr}")
      sys.exit(1)
    finally:
      # 一時設定ファイルを削除
      if os.path.exists(temp_export_settings_file):
        os.remove(temp_export_settings_file)

  # root.mainloop()

  messagebox.showinfo(msgbox_title, "Spineのエクスポート処理が終了しました。")
  sys.exit(0)


if __name__ == '__main__':
  main()
