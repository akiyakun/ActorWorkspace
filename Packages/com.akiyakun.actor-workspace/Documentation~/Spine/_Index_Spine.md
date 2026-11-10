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

`json_to_spine`・`spine_to_json` は JSON を原本にするプロジェクト用です。ツールは `External~/tools/`(プロジェクトの `tools/` にコピーして使う)にあります。Python 3 と Spine が要ります。`bin/` の `.bat` にフォルダをドラッグ&ドロップするか、`tools/` で `python spine/<スクリプト>.py --inputs <フォルダ>` を実行します。

| ツール | 内容 |
| --- | --- |
| `json_to_spine` | 原本の JSON から、同じフォルダに作業用の `.spine` を作る。もう `.spine` があるときは作らない |
| `spine_to_json` | `.spine` から原本の JSON を作る。そのフォルダの原本と名前が違う `.spine`、画像・音のフォルダが絶対パスの `.spine` ではエラー。スケルトンの名前が `skeleton` のままなら警告。hash 以外が同じなら原本を書き換えない |
| `spine_actor_export_to_unity` | Unity 用のデータを書き出す。フォルダに原本の JSON があれば、一時的な `.spine` に取り込んでから書き出す(JSON から直接アトラスを作ると、連番の画像が見つからないため)。無ければ、前の版と同じく `.spine` から書き出す。`.spine` を直接渡したときは、その `.spine` から書き出す(原本の JSON もあれば警告)。書き出しの設定は `bin/spine_export_setting.json` があればそれ、無ければ `spine/default_spine_export_setting.json` |

* JSON を原本にするプロジェクトでは、`.spine` を `.gitignore` で無視してください。作業用の `.spine` と、`spine_actor_export_to_unity` が原本と同じフォルダに作る一時的な `_export_temp.spine`(書き出しが途中で止まると残る)をコミットしないためです
* `spine_actor_export_to_unity` は、フォルダに `.spine` と JSON の両方があると JSON を原本として使い、そのことを表示します。`.spine` を原本にするプロジェクトでは、原本のフォルダに書き出した JSON を置かないでください
* `override_export_setting.json`・`extra_data.json` は、前の版と同じく下のフォルダにあっても使います(フォルダの直下を優先)
* 共通の処理は `spine/spine_common.py` にあります
* `bin/` の `.exe`・`.app` は、`.spine` を入力にしていた前の版をビルドしたものです(作り直していない)

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









