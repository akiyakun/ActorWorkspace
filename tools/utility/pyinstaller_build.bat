@echo off

set build_dir=..¥..¥build¥pyinstaller

set name=symlink
call :BUILD

pause
exit



:BUILD

@REM 実行ファイルのビルド
pyinstaller .¥%name%.py --onefile --clean --name %name% --distpath %build_dir%¥dist --workpath %build_dir%¥build --specpath %build_dir%¥build¥

@REM 実行ファイルをbinフォルダにコピー
xcopy /E /I /Y %build_dir%¥dist ..¥bin¥

exit /b
