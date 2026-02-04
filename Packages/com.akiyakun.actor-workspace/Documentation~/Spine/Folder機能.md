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











