@rem うえからした (sjis認識させる呪文)
@echo off

@rem この.batが置かれているディレクトリをカレントにする
cd /d %‾dp0
cd ..¥
echo %CD%

@rem Spine.exeのパス
set SPINE_PATH=C:¥Program Files¥Spine¥Spine.exe

.¥bin¥spine_actor_export_to_unity.exe --spine_path "%SPINE_PATH%" --inputs %*

set EXIT_CODE=%ERRORLEVEL%
if not "%EXIT_CODE%"=="0" (
	echo エラーが発生しました。何かキーを押すと終了します。
	pause >nul
)

exit /b %EXIT_CODE%