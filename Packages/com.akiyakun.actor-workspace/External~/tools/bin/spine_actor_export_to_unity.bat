@rem うえからした (sjis認識させる呪文)
@echo off

@rem この.batが置かれているディレクトリをカレントにする
cd /d %‾dp0

@rem Spine.exeのパス
set SPINE_PATH=C:¥Program Files¥Spine¥Spine.exe

.¥spine_actor_export_to_unity.exe --spine_path "%SPINE_PATH%" --inputs %*

pause