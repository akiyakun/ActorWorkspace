@echo off

@REM Chaange current directory to the directory of this script
cd /d %‾dp0
@REM echo %cd%

py run_tests.py --runner Windows --testPlatform %1 --assemblyNames %2
echo run_tests.py return code %ERRORLEVEL%
exit /b %ERRORLEVEL%

@REM echo end
@REM pause
