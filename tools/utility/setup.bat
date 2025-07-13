@REM @echo off

@REM 管理者権限で実行する
openfiles > nul
if errorlevel 1 (
    PowerShell.exe -Command Start-Process ¥"%‾f0¥" -Verb runas
    exit
)


@REM gitのルートディレクトリに移動
cd /d %‾dp0
@REM echo %cd%


set src=..¥..¥..¥libs¥afl
if exist "%src%" (
    ..¥bin¥symlink.exe --override --src "%src%" --dest "../../Packages/cyou.sumomo.afl"
)

set src=..¥..¥..¥libs¥unitycicd
if exist "%src%" (
    ..¥bin¥symlink.exe --override --src "%src%" --dest "../../Packages/com.akiyakun.unitycicd"
)


@REM echo success
pause
