@echo off
rem Make the original Spine JSON from a .spine project.
rem Drag and drop files or folders onto this file. Requires Python 3 and Spine.
chcp 65001 >nul
set PYTHONIOENCODING=utf-8
python "%‾dp0..¥spine¥spine_to_json.py" --inputs %*
set EXIT_CODE=%ERRORLEVEL%
pause
exit /b %EXIT_CODE%
