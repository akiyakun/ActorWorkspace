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









