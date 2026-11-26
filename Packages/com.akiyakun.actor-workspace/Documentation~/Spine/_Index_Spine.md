[TOC]

# Overview

ActorWorkspaceパッケージ、以下AWと略します。



# 流れ

UnitySpineSettings (ScriptableObject) を作成します。
パッケージが使用する設定データ(インポート設定や自動処理用の設定など)の情報が設定されます。

作成方法はProjectウィンドウの右クリックメニューから
`App/ActorWorkspace/UnitySpineSettings`
を実行し設定ファイルを追加してください(/Assets/Settings 等に)



## Spine のデータの原本と書き出しツール

Spine のデータの原本は、`.spine` か、Spine から書き出した JSON(Nonessential data あり)のどちらかにします(プロジェクトごとに決める)。JSON にすると、AI エージェントが中身を読んだり直したりでき、差分も読めます。JSON を原本にするときは、`.spine` は原本から作り直せる作業用のファイルとして扱います。

* 原本のファイル名はスケルトンの名前にします(JSON を `.spine` に取り込むと、ファイル名がスケルトンの名前になる)
* JSON にして失われるのは、作業するときだけの情報(最後に書き出した先などの設定、書き出さない設定にしたもの)です。ボーンの色・アイコン、メッシュの辺は残ります(Spine 4.2 で確認)

`json_to_spine`・`spine_to_json` は JSON を原本にするプロジェクト用です。ツールは `External~/tools/`(プロジェクトの `tools/` にコピーして使う)にあります。Python 3 と Spine が要ります。`bin/` の `.bat` にフォルダをドラッグ&ドロップするか、`tools/` で `python spine/<スクリプト>.py --inputs <フォルダ>` を実行します。`bin/spine_actor_export_to_unity.bat` は、Python の要らない `bin/` の `.exe`(`spine/spine_actor_export_to_unity.py` をビルドしたもの)を呼びます。

| ツール | 内容 |
| --- | --- |
| `json_to_spine` | 原本の JSON から、同じフォルダに作業用の `.spine` を作る。もう `.spine` があるときは作らない |
| `spine_to_json` | `.spine` から原本の JSON を作る。そのフォルダの原本と名前が違う `.spine`、画像・音のフォルダが絶対パスの `.spine` ではエラー。スケルトンの名前が `skeleton` のままなら警告。hash 以外が同じなら原本を書き換えない |
| `spine_actor_export_to_unity` | Unity 用のデータを書き出す。フォルダに原本の JSON があれば、一時的な `.spine` に取り込んでから書き出す(JSON から直接アトラスを作ると、連番の画像が見つからないため)。無ければ、前の版と同じく `.spine` から書き出す。`.spine` を直接渡したときは、その `.spine` から書き出す(原本の JSON もあれば警告)。書き出しの設定は `bin/spine_export_setting.json` があればそれ、無ければ `spine/default_spine_export_setting.json` |

* JSON を原本にするプロジェクトでは、`.spine` を `.gitignore` で無視してください。作業用の `.spine` と、`spine_actor_export_to_unity` が原本と同じフォルダに作る一時的な `_export_temp.spine`(書き出しが途中で止まると残る)をコミットしないためです
* `spine_actor_export_to_unity` は、フォルダに `.spine` と JSON の両方があると JSON を原本として使い、そのことを表示します。`.spine` を原本にするプロジェクトでは、原本のフォルダに書き出した JSON を置かないでください
* `override_export_setting.json`・`extra_data.json` は、前の版と同じく下のフォルダにあっても使います(フォルダの直下を優先)
* 共通の処理は `spine/spine_common.py` にあります
* Spine の場所は、`--spine_path`、環境変数 `SPINE_PATH`、Unity プロジェクトのフォルダ(`tools/` の 1 つ上)の `.env` に書いた `SPINE_PATH`、標準のインストール先の順に探します。`.env` は PC ごとのファイルなので Git に入れません(afl の `.gitignore` で無視)。`bin/spine_actor_export_to_unity.bat` も、環境変数が無ければ `.env` を読んで `.exe` に渡します。`.bat` は `=` の前後の空白、値の後ろのコメント、日本語のパスを読めないので、`.env` は `SPINE_PATH=<Spine の実行ファイル>` の形で書きます
* 書き出し先のフォルダの名前の `=` から `@` の前まではコメントです(afl の `AutoAddressing` がアドレスから除く。例: `Enemy_5020=ダイエイユー@bundle` のアドレスは `Enemy_5020@bundle`)。`spine_actor_export_to_unity` は、コメントを除いた名前が同じで名前が違うフォルダが書き出し先にあれば、元データのフォルダの名前に変えて(`.meta` も一緒に)から上書きします。元データのフォルダにコメントを足した・変えた・消したときも、同じアセット(GUID)を上書きできます。コメントを除いた名前が同じフォルダが書き出し先に 2 つ以上あるときはエラーにします
* `spine_actor_export_to_unity` は、Unity が開いているときは書き出しの間 Unity の取り込み・保存を止めます(afl の ExternalEditLock。`tools/lib/unity_external_edit_lock.py` を使う)。Unity が止まったことを確かめられないときは、フォルダの名前を変えずにエラーにします(Unity のメニューの一括書き出しを使うか、Unity で名前を変えてから書き出す)
* `spine_actor_export_to_unity` は、人が使うとき(`.bat` にドラッグ&ドロップしたときなど)はダイアログを出します。入力が 2 つ以上なら開始の確認、書き出しの間は最前面に進み具合の窓、終わったら終了の知らせ、失敗したらエラーの内容です。開始の確認と進み具合の窓には、書き出しの間 Unity を操作しないように赤い字で注意を出します。ダイアログは `spine/export_dialog.py`(tkinter だけで作る)。書き出しは別のスレッドで走らせるので、書き出しの間も窓は固まりません
* 自動化で使うとき(Unity のメニューの一括書き出し、AI エージェントなど)は `--no_dialog` を付けます。ダイアログを出さずに(tkinter を使わずに)、コンソールだけで動きます。tkinter が使えない(ディスプレイが無いなど)ときも、コンソールだけで続けます。どちらでも、エラーは `[エラー]` で始まる行を標準エラー出力に出し、終了コード 1 で終わります(最初のエラーで止まる)
* `bin/` の `.exe` は、`spine/spine_actor_export_to_unity.py` を PyInstaller でビルドしたものです(デザイナーの PC には Python が入っていないことがあるため)。ビルドは、プロジェクトの `tools/` で `spine/spine_actor_export_to_unity_build.bat` を実行します(PyInstaller が要る。`tools/lib/` の `unity_external_edit_lock.py` も一緒に入れるため、afl の `tools/lib/` があるプロジェクトの `tools/` でビルドする)。`bin/` に入った `.exe` を `External~/tools/bin/` にコピーします
* Mac の `bin/spine_actor_export_to_unity`・`.app` は、`.spine` を入力にしていた前の版をビルドしたものです(作り直していない)

### 一括書き出し(Unity のメニュー)

`App/Develop/ActorWorkspace/Spine/Export All Spine To Unity` で、すべての Spine を Unity 用に書き出して取り込みます(`Editor/Integration/UnitySpine/SpineBulkExporter.cs`)。Python 3 と Spine が要ります。

* 対象は、UnitySpineSettings の `Export Source Directory`(既定は `../spine`。Unity プロジェクトのフォルダからの相対パス)の下の `<カテゴリ>/<名前>@bundle` のフォルダです。名前が `@bundle` で終わらないフォルダ(カットインなど UI の Spine)は書き出しません
* 書き出し先は `Export Destination Directory`(既定は `Assets/AssetBundleData/Actor`)の下の `<カテゴリ>/<フォルダの名前>` です。`ImportTargetDirectories` に入っている場所にしてください(入っていないと取り込みの処理が動かず、エラーで止まります)
* 1 件ずつ、`tools/spine/spine_actor_export_to_unity.py` で書き出し、取り込み(`SpineAssetPostprocessor` とプロジェクト側の `ImportCallback` を含む)が終わるのを待ってから次へ進みます
* 書き出す前に、書き出し先のフォルダの名前を元データのフォルダの名前に合わせます(上のコメントの決まり)。Unity の中で `AssetDatabase.MoveAsset` で変えるので、GUID は変わりません
* 書き出しの間は、afl の ExternalEditLock で取り込み・保存・ドメインリロードを止めます
* 次のどれかがあると、その件で止めます。書き出しのスクリプトが失敗した、その件の間にエラーログが出た、書き出し先に SkeletonDataAsset・SpineExtraData が無い、SpineExtraData の `IsImportError` が立っている。どれが原因かと、スクリプトの出力を Console に出します
* キャンセルは書き出しの間のプログレスバーから。今の件が終わってから止めます
* 実行中にスクリプトを変えないでください。ドメインリロードが起きると、そこで黙って止まります

## インポート

AWの SpineAssetPostprocessor によってSpineファイルのインポート後のポストプロセスが行われます。

このとき SpineExtraDataScriptableObject ファイルが自動的に作成されます。
SpineExtraDataはAWで使うカスタムデータの入れ物です。
自動生成されるものなので手動で設定・変更しないでください。

### SkeletonAnimation

### SkeletonMecanim

## SpineのGameObject作成

AWで使うためのコンポーネントをSpineのコンポーネントと一緒にアタッチする必要があります(手動)

### SkeletonAnimation

SpineSkeleton コンポーネントを追加してください。

SpineExtraData に インポート時に自動生成された SpineExtraDataScriptableObject アセットの参照を設定してください。

### SkeletonMecanim

SpineMecanim コンポーネントを追加してください。

同時に SpineMecanimAnimationEventDetector コンポーネントも追加されます。
こちらはSpineのアニメーションイベントをAnimationClipから取得するためのコンポーネントです。

SpineExtraData に インポート時に自動生成された SpineExtraDataScriptableObject アセットの参照を設定してください。

## スキン

Spine のアクター(SpineActorBase)は、実行中にスキンを変えられます。SkeletonAnimation と SkeletonMecanim のどちらでも同じです。

| API | 内容 |
| --- | --- |
| `IAWActor.SkinList` | スケルトンのスキンの一覧(SkeletonData のスキン。デフォルトのスキンも入る) |
| `IAWActor.SkinIndices` | 今付けているスキンの番号(SkinList の番号)。合成しているときは合成した順。SkinList に無いスキンを付けているときは空 |
| `IAWActor.SetSkin(int)` | スキンを 1 つ付ける |
| `IAWActor.SetSkins(IReadOnlyList<int>)` | 複数のスキンを合成して付ける。同じスロットの同じ名前のアタッチメントは、後ろのスキンのものを使う。空ならスキンなし(デフォルトのスキンだけ) |
| `SpineActorBase.SetSkin(Spine.Skin)` | SkinList に無いスキン(実行時に作ったスキンなど)を付ける |
| `IAWActor.Restore()` | 初期化したときのスキン(SkeletonMecanim・SkeletonAnimation の Initial Skin)に戻す |

* 合成するときは、新しい `Spine.Skin` を作って `AddSkin` します。SkeletonData のスキンはほかのアクターと共有しているので書き換えません。
* `SetSkin(Spine.Skin)` に同じ `Skin` のインスタンスを渡し直しても、Spine は何もしません。中身を変えたスキンは、新しく作って渡してください。
* 見た目への反映
  * 今付いているアタッチメントは、すぐに新しいスキンの同じ名前のアタッチメントに置き換わります。再生中のアニメーションが付けた絵(例えば dig のコマ)も、そのまま新しいスキンの絵になります。シーケンスのコマも引き継ぎます。
  * スロットをセットアップポーズに戻す(`Skeleton.SetSlotsToSetupPose`)のは、次にアニメーションを当てる直前(SkeletonAnimation・SkeletonMecanim の `BeforeApply`)です。スキンを変えた直後に戻すと、次の更新までアニメーションの絵が待機の絵などに戻るためです。
  * スキンを変えたあとにアニメーションを当て直すことはしません。同じフレームでイベント(SE など)やルートモーションが二重になるためです。
  * そのため、新しいスキンに同じ名前のアタッチメントが無いスロットは、次の更新まで前のスキンの絵のままです。メッシュのデフォーム(Deform)も、次の更新まではセットアップの形になります。
  * スキンを変えたあと、次にアニメーションを当てるまでにスケルトンを直接変えた内容(`Skeleton.SetAttachment` やスロットの色など)は、セットアップポーズに戻って消えます。直接変えるときは、アニメーションを当てたあと(`UpdateLocal` など)にしてください。
  * 今と同じスキンを付けたときは何もしません(セットアップポーズにも戻しません)。
* SimpleSprite などスキンの合成に対応しないアクターは、`SetSkins` に 1 つだけ渡したときに `SetSkin` を呼びます。
* アクタービューワーでは、スキンの一覧をクリックするとスキンを付け、Shift を押しながらクリックすると今のスキンに足して合成します(選んでいるスキンなら外します。最後の 1 つは外しません)。付けているスキンは、名前の前に合成する順の番号([1] など)を付けて太字で表示します。









