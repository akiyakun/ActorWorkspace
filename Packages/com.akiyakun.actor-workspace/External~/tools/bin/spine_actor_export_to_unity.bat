@rem うえからした (sjis認識させる呪文)
@echo off

@rem この.batが置かれているディレクトリをカレントにする
cd /d %‾dp0
cd ..¥
echo %CD%

@rem Spine の場所は exe が探す(環境変数 SPINE_PATH、Unity プロジェクトのフォルダ（tools の 1 つ上）の .env に書いた SPINE_PATH、標準のインストール先の順)
.¥bin¥spine_actor_export_to_unity.exe --inputs %*

set EXIT_CODE=%ERRORLEVEL%
if not "%EXIT_CODE%"=="0" (
	echo エラーが発生しました。何かキーを押すと終了します。
	pause >nul
)

exit /b %EXIT_CODE%