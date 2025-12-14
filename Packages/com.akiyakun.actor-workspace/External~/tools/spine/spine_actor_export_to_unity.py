#!/usr/bin/env python
#
import sys
import os
import os.path
import argparse
import shutil
import filecmp
import json

import subprocess
import time
import tkinter as tk
from tkinter import messagebox
from tkinter import ttk
from send2trash import send2trash

msgbox_title = "spine_actor_export_to_unity"
export_dir = "../../Assets/AssetBundleData/Actor/"
spine_export_settings_file = "../spine/default_spine_export_setting.json"
temp_export_settings_file = "./_temp_export_setting.json"
override_export_setting_filename = "override_export_setting.json"


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


def get_spine_filelist(inputs):
  ret = []

  for i in range(len(inputs)):
    # if i == 0: continue
    # messagebox.showinfo('メッセージ', sys.argv[i])

    # .spineファイルであればそのままリストに追加
    if inputs[i].endswith(".spine"):
      ret.append(inputs[i])
      continue

    # ディレクトリであれば、そのディレクトリ以下の.spineファイルを全てリストに追加
    for root, dirs, files in os.walk(inputs[i]):
      for file in files:
        if file.endswith(".spine"):
          ret.append(os.path.join(root, file))

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
# python3 spine_actor_export_to_unity.py --spine_path /Applications/Spine.app/Contents/MacOS/Spine --inputs /Spine/Player/Player_0001@bundle/Player.spine
if __name__ == '__main__':
  args = parse_args()
  export_dir = args.export_dir
  spine_export_settings_file = args.spine_export_setting

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

  # カレントディレクトリを設定
  os.chdir(os.path.dirname(os.path.abspath(sys.argv[0])))

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
    input_path = spine_files[i]
    # e.g. /Spine/Player/Player_0001@bundle/Player.spine

    category_dir_name = os.path.basename(os.path.dirname(os.path.dirname(input_path)))
    # Player

    output_path = os.path.join(export_dir, category_dir_name)
    # print(f"test: {os.path.basename(os.path.dirname(input_path))}")
    output_path = os.path.join(output_path, os.path.basename(os.path.dirname(input_path)))
    print(f"output_path: {output_path}")
    os.makedirs(output_path, exist_ok=True)
    # sys.exit(1)

    # messagebox.showinfo('メッセージ', input_path)

    # プログレスバー表示変更
    label1.configure(text=f"{input_path}")
    progbar.configure(value=i/len(spine_files))
    progbar.update()

    # 今回使用する設定ファイルをコピー
    shutil.copy(spine_export_settings_file, temp_export_settings_file)

    # オーバーライド設定ファイルがあればマージ
    override_path = os.path.join(os.path.dirname(input_path), override_export_setting_filename)
    if os.path.isfile(override_path):
        # print(f"Override found. Merging: {override_path}")
        merge_json_files(temp_export_settings_file, override_path)

    try:
      # See also: https://ja.esotericsoftware.com/spine-command-line-interface
      result = subprocess.run(
        # 実行したいコマンド（例: ls -l）
        # "/Applications/Spine.app/Contents/MacOS/Spine",
        [
          args.spine_path,
          "--update", "4.2.xx",
          "--input", f"{input_path}",
          "--output", f"{output_path}",
          "--export", temp_export_settings_file
        ],
        capture_output=True,         # 標準出力・標準エラー出力を取得
        text=True,                   # 出力を文字列として取得
        check=True
      )
    except subprocess.CalledProcessError as e:
      messagebox.showerror(msgbox_title, f"エクスポート失敗: {input_path}\n\n{e.stderr}")
      sys.exit(1)
    finally:
      # 一時設定ファイルを削除
      if os.path.exists(temp_export_settings_file):
        os.remove(temp_export_settings_file)

  # root.mainloop()

  messagebox.showinfo(msgbox_title, "Spineのエクスポート処理が終了しました。")
  sys.exit(0)
