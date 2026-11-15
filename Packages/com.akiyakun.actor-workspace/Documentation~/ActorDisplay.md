[TOC]

# ActorDisplay

`AWActorDisplay` はアクターの表示のルートです。アクター全体のスケールや左右の反転を受け持ちます。

## 左右の反転

`SetScaleSign` で `localScale` の符号を変えて反転します（`ValueSignType.NegativeX` で左向き）。`ResetScale` で (1,1,1) に戻します。

子のスケールもマイナスになるので、`ActorDisplay` の子に 3D の当たり判定を置くときは、下の `AWKeepPositiveScale` を付けます。

## AWKeepPositiveScale

親の反転（マイナスのスケール）を打ち消して、ワールドのスケール（`lossyScale`）をプラスに保つコンポーネントです。

3D の `BoxCollider` はマイナスのスケールに対応していません。次の条件がそろうと、物理の更新のときに `BoxCollider does not support negative scale or size.` の警告が出ます。

- `BoxCollider` がトリガーではない
- kinematic ではない `Rigidbody` に付いている
- `lossyScale` がマイナス

2D の `BoxCollider2D` では出ません。

### 使い方

- `ActorDisplay` の子の、3D の当たり判定の GameObject に付けます。
- `AWActorDisplay` が初期化（`InitializeAsync`）のときに子から集め、`SetScaleSign`・`ResetScale` でスケールを変えるたびに `Apply()` を呼びます。
  - 反転したその場で直すためです。毎フレームの更新で直すと、反転と同じフレームの物理の更新に間に合わないことがあります。
  - 初期化のあとで足した子は対象になりません。

### 注意

- 自分の `localScale` は符号だけを書き換えます。わざとマイナスにしていても、`lossyScale` がプラスになるように直されます。
- 自分の向きは反転しなくなるので、`BoxCollider.center` のずれは左右反転しません。`center` は 0 にして、位置は GameObject（や `PositionConstraint`）で合わせます。
- 自分のスケールを毎回書き込むコンポーネント（Spine の `BoneSlotFollower` など）と一緒に使うと、書き込みで打ち消しが消えます。`BoundingBox3DFollower` の当たり判定は、今は打ち消せません（[Folder機能](Spine/Folder機能.md)）。
