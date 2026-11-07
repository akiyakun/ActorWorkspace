[TOC]

# Overview

ActorWorkspaceパッケージ、以下AWと略します。



# 流れ

UnitySpineSettings (ScriptableObject) を作成します。
パッケージが使用する設定データ(インポート設定や自動処理用の設定など)の情報が設定されます。

作成方法はProjectウィンドウの右クリックメニューから
`App/ActorWorkspace/UnitySpineSettings`
を実行し設定ファイルを追加してください(/Assets/Settings 等に)



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









