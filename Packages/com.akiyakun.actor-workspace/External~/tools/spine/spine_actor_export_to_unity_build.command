cd "$(dirname "$0")"

# tkinter が動くか確かめる(動かない Python でビルドすると、ダイアログの出ない実行ファイルになり、コンソールだけで動くので気付きにくい)
# MEMO: pyinstaller と同じ Python が python3 で呼ばれる前提(venv を有効にしてから実行する)。PyInstaller が入っているかも見る
# MEMO: Tk() ではなく Tcl() で確かめる(SSH などで画面が無くてもビルドできるように)。Tk 8.5(macOS の Python)は表示が崩れるので断る
if ! python3 -c "import sys, tkinter, PyInstaller; tkinter.Tcl(); sys.exit(0 if tkinter.TkVersion >= 8.6 else 'Tk ' + str(tkinter.TkVersion) + ' is too old')"; then
  echo "[エラー] python3 の tkinter が動かない(Tk 8.6 以上でない)か、PyInstaller が入っていないので、ビルドしませんでした。tkinter の動く Python でビルドしてください(ActorWorkspace の Documentation~/Spine/_Index_Spine.md)"
  read -p "Press any key to exit..."
  exit 1
fi

cd ../utility
# echo ${PWD}
./pyinstaller_builder.sh spine_actor_export_to_unity ../spine/spine_actor_export_to_unity.py
read -p "Press any key to exit..."
