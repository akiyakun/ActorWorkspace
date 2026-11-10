@echo off
rem Export Unity data (JSON, atlas) from the original Spine JSON.
rem Drag and drop files or folders onto this file. Requires Python 3 and Spine.
chcp 65001 >nul
set PYTHONIOENCODING=utf-8
python "%‾dp0..¥spine¥spine_actor_export_to_unity.py" --inputs %*
set EXIT_CODE=%ERRORLEVEL%
pause
exit /b %EXIT_CODE%
