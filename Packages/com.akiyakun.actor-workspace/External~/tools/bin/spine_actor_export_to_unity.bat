@rem うえからした (sjis認識させる呪文)
@echo off

@rem この.batが置かれているディレクトリをカレントにする
cd /d %‾dp0
cd ..¥
echo %CD%

@rem Spine.exeのパス
@rem 環境変数 SPINE_PATH が無ければ、Unity プロジェクトのフォルダ（tools の 1 つ上）の .env に書いた SPINE_PATH を使う
if not defined SPINE_PATH if exist "..¥.env" (
	for /f "usebackq eol=# tokens=1,* delims==" %%a in ("..¥.env") do (
		if /i "%%a"=="SPINE_PATH" set "SPINE_PATH=%%‾b"
	)
)
@rem どちらにも無ければ、標準のインストール先
if not defined SPINE_PATH set "SPINE_PATH=%ProgramFiles%¥Spine¥Spine.exe"

.¥bin¥spine_actor_export_to_unity.exe --spine_path "%SPINE_PATH%" --inputs %*

set EXIT_CODE=%ERRORLEVEL%
if not "%EXIT_CODE%"=="0" (
	echo エラーが発生しました。何かキーを押すと終了します。
	pause >nul
)

exit /b %EXIT_CODE%