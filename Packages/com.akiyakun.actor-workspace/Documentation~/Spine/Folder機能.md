[TOC]

# Overview

Folderと名前が付いていますが、OSのフォルダーなどとは一切関係ありません。
`Folder機能`はSpineの特定の名前のボーンに意味やルールを持たせ、
インポート時やRuntime実行時などに特殊な処理(UnityのGameObjectをアタッチしたりなど)を行える機能セットの名称です。

# Folder

https://game.capcom.com/cfn/sfv/column-130393.html

| No   |                    |                                        |
| ---- | ------------------ | -------------------------------------- |
| 3    | CollisionBoxFolder | 押し合い判定や、PushBoxと言われるやつ  |
| 4    | HurtBoxFolder      | やられ判定や、喰らい判定と言われるやつ |
| 5    | HitBoxFolder       | 攻撃判定                               |
|      |                    |                                        |
| 8    | EffectFolder       | エフェクト用                           |
|      |                    |                                        |

# Follower

対象に行う実際の処理

## BoundingBoxFollower

https://ja.esotericsoftware.com/spine-unity-utility-components#BoundingBoxFollower

Spine標準の境界ボックスコンポーネント


```
注意: 頂点変形アニメーション(境界ボックスの頂点をアニメーションで時間軸に従って移動させること)に対しては追従せず、初期形状のみを対象としています。
```

```
注意: ボーンの位置は自動的には追従しません。そのため、通常は BoneFollower コンポーネントと一緒に使用します。BoundingBoxFollower のInspectorの Add Bone Follower ボタンを使用して BoneFollower コンポーネントの作成と設定を行うことができます。
```

境界ボックストランスレートもだめ
-> slotの親をboneにし、slotと同じ名前にしてBoneFollowerをアタッチすればできる

スロットの表示・非表示は可能

## BoneSlotFollower

BoneとSlotを同一に扱う(扱いたい)自前実装のコンポーネント
Boneのトランスフォームを使いたい、Slotのアタッチメントの表示状態も使いたい、というときに。

トランスフォームをアニメーションしたい場合Boneを使うことになるので基本的に以下のツリー構造になる。

```
Box1という名前の場合
[]はノードの種類

Box1[Bone]
  Box1[Slot]
    Image
```



## BoundingBox2DFollower

3D版と区別するためにBoneSlotFollowerを継承しただけのクラス。

## BoundingBox3DFollower

2D版と区別するためにBoneSlotFollowerを継承しただけのクラス。

* `BoxCollider` の大きさは、ボーンと同じ名前のスロットに付いている四角い画像（`RegionAttachment`。判定の箱の目印の画像）の大きさと位置に合わせます。これにボーンのスケールが掛かります。画像が無いときは今までどおり 32 ピクセル(SkeletonData のスケール 0.01 で 0.32)の箱です
* `ActorDisplay` で左右を反転すると、作った `BoxCollider` の `lossyScale` がマイナスになり、物理の更新で警告が出ます（[ActorDisplay](../ActorDisplay.md) の `AWKeepPositiveScale`）。そのため、3D版の `BoneSlotFollower` は `keepPositiveWorldScale` で、親が反転していてもワールドのスケールをプラスに保ちます。2D版（`BoxCollider2D`）では警告は出ません
* **注意：箱の画像はボーンの真ん中に置き、回さないでください。** 位置や向きはボーンのほうで動かします。ワールドのスケールをプラスに保つので、画像をボーンから左右にずらしたり回したりすると、左右反転したときに判定の位置が左右逆になりません（上下のずれは問題ありません）
  * 取り込みのとき、`SpineAssetPostprocessor.ValidateFolderBoxes` が、3D版のフォルダの箱の画像（どのスキンのものも）がボーンの真ん中にあるか確かめ、ずれていればエラーログを出します（0.5 ピクセルまでは許す。ゲームでは使えるので、取り込みエラーにはしない）。一括書き出し（`SpineBulkExporter`）では、そのアクターで止まります

## フォローオブジェクトの下でエフェクトを出すとき（SpineFollowerUtility）

Spine の `BoneFollower`・`PointFollower` は `LateUpdate` で骨の位置に動きます。Spine のイベントや Mecanim のアニメーションイベントは `LateUpdate` より前に来るので、その中でフォローオブジェクトの下にエフェクトを出すと、フォローオブジェクトはまだ前のフレームの骨の位置にいます。World 空間の粒は `LateUpdate` より前（パーティクルの計算の始まり）の位置で出るので、1 フレーム前の骨の位置に出てしまいます。

* 出す直前に `SpineFollowerUtility.UpdateFollower(フォローオブジェクト)` を呼ぶと、今の骨の位置にすぐ動きます（`LateUpdate()` を呼ぶ。本来の `LateUpdate` でもう一度動くが、同じ位置になる）
* 骨のワールドの位置が今のフレームのものになってから呼びます（Spine の `Update` のあと）
* `BoneSlotFollower`（と BoundingBox の Follower）は `UpdateComplete` で動くので、呼ばなくても今の位置にいます（呼んでも何もしません）

## 判定の箱の目印の画像

判定の箱（`BoundingBox2DFollower`・`BoundingBox3DFollower`）のスロットの画像は、ゲームでは出しません。`BoneSlotFollower` の `hideSlot` で、毎回の更新でスロットの色のアルファを 0 にします。

* アタッチメントを外すと、`BoneSlotFollower` がフォロワーを非アクティブにする（判定が消える）ので、アタッチメントは付けたまま、色で消しています
* デバッグで当たり判定を見たいときは、`UnitySpineSettings` の `Show Folder Slots` を ON にするか、実行中に `SpineFolderSlotVisibility.IsVisible = true` にします（次の更新から反映。`Reset()` で設定の値に戻す）。出すときのアルファはセットアップの値(`SlotData`)です
* ゲーム中に当たり判定(HurtBox・HitBox)を色分けで描くのは `ALCollisionBoxVisualizer`(ArcaneLedger。[ALCollisionBoxVisualizer](../ArcaneLedger/ALCollisionBoxVisualizer.md))です。色は `UnitySpineSettings` の `Collision Box Visualizer` に置き、ゲーム側が読んで渡します(下の「Collision Box Visualizer」)

# Spineデータ側の設定

ツリービュー上で分かりやすく整理する目的で名前の頭に番号を付けます

```
3_CollisionBoxFolder
4_HurtBoxFolder
5_HitBoxFolder
```

# UnitySpineSettings

UnitySpineSettings の `Folder Settings`リストに設定を追加します。
現在のプロジェクトに対して、対象のFolder名(ボーン名)に対してどういう処理を行うのかセットする感じです。

## Collision Box Visualizer

ゲーム中に当たり判定(HurtBox・HitBox)を描くときの色です。AW のこの設定は色を持つだけで、描くのは `ALCollisionBoxVisualizer` です。`ALCollisionBoxVisualizer` は設定ファイルを読まないので、ゲーム側がこの値を読んで渡します(`HurtBoxColor = settings.GetHurtBoxColor` など)。

| 項目 | 内容 |
| --- | --- |
| Hurt Box Colors | HurtBox(やられ判定)の色。レイヤーごと(`Layers` に入っているレイヤーのコライダーをその色にする。リストの上から見て、最初に入っているもの) |
| Hurt Box Default Color | HurtBox の色(どのレイヤーの設定にも入らないとき) |
| Hit Box Colors | HitBox(攻撃判定)の色。レイヤーごと |
| Hit Box Default Color | HitBox の色(どのレイヤーの設定にも入らないとき) |
| Effect Hit Box Color | エフェクト(パーティクルの弾など)の HitBox の色 |
| Hit Box Fill Alpha | HitBox の塗りの不透明度(色のアルファに掛ける)。HurtBox は塗らずに枠線だけ描く想定 |
| Collision Box Line Width | 枠線の太さ(画面のピクセル) |

* 色はコードからは `GetHurtBoxColor(layer)`・`GetHitBoxColor(layer)` で取る
* HurtBox か HitBox かは、`ALCollisionExtraData.BoxKind` で見分けられる(`ALHurtBoxBehaviour`・`ALHitBoxBehaviour` が Awake で `HurtBox`・`HitBox` を入れる。ほかで付けたものは `Unknown`)

## Folder Settings リスト

### Folder Name

対象のFolder名(ボーン名)

### Follower Type

使用するFollower

### Layer Setting

追加されるFollowerのGameObject以下にLayerを再設定するための方法を選択します。

|                |                                                              |      |
| -------------- | ------------------------------------------------------------ | ---- |
| Same As Parent | 追加されるFollowerのGameObjectに、親と同じLayerを設定する。  |      |
| Per Layer      | 追加されるFollowerのGameObjectに、Layer毎に細かく設定をする。<br />後述の`Layer Pair List`にも設定が必要です。 |      |
|                |                                                              |      |

#### Layer Pair List

* Source Layer

  対象になる親GameObjectのLayer

* Target Layer
  設定されるLayer

例：
Source Layer = Player, Target Layer = PlayerHitBox
Source Layer = Enemy, Target Layer = EnemyHitBox
とするとPlayerにはプレイヤー用のLayerが再設定され、Enemyには敵用のLayerが再設定されます。











