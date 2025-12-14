@rem うえからした (sjis認識させる呪文)
@echo off

cd /d "%‾dp0..¥utility"
echo %CD%
./pyinstaller_builder.bat spine_actor_export_to_unity ..¥lib¥spine_actor_export_to_unity.py

pause