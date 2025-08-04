@REM @echo off

start /WAIT "" "C:¥Program Files¥Unity¥Hub¥Editor¥6000.0.50f1¥Editor¥Unity.exe" -runTests -batchmode -projectPath "../../" -testResults "build/test_results.xml" -testPlatform PlayMode
echo %ERRORLEVEL%

@REM echo success
pause
