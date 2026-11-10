@echo off
rem Make a working .spine project from the original Spine JSON.
rem Drag and drop files or folders onto this file. Requires Python 3 and Spine.
chcp 65001 >nul
set PYTHONIOENCODING=utf-8
python "%‾dp0..¥spine¥json_to_spine.py" --inputs %*
set EXIT_CODE=%ERRORLEVEL%
pause
exit /b %EXIT_CODE%
