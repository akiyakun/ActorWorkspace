# 当たり判定を画面に描く（ALCollisionBoxVisualizer）

ゲーム中に、ArcaneLedger の当たり判定（HurtBox・HitBox・エフェクトの HitBox）を色分けで画面に重ねて描くデバッグ表示です。
`Runtime/ArcaneLedger/Collider/ALCollisionBoxVisualizer.cs`

* 最終更新：2026-11-26（作った。描く仕組みは afl の `CollisionBoxVisualizer`）

## 何のためか

敵の攻撃が広すぎるときなどに、当たりがどこにあるか、直すのがエフェクトか Spine のモーションかを見分けるためのものです。

## しくみ

描く仕組みは afl の `CollisionBoxVisualizer`（`afl.Service.Debugger`。afl の `docs~/InAppDebug/_Index_InAppDebug.md` の「関連する機能」）です。`ALCollisionBoxVisualizer` はその派生で、描く対象を集めて種類に分け、色を決めます。

* 描く対象（`RefreshInterval` 秒ごとに、非アクティブのものも含めて集め直す。描くのは `activeInHierarchy` で `enabled` のものだけ）
  * `ALCollisionExtraData`：`BoxKind` が `HurtBox`・`HitBox` のもの（`ALHurtBoxBehaviour`・`ALHitBoxBehaviour` が Awake で入れる）。`Unknown` のものは `ClassifyUnknown`（virtual。既定は描かない）で決める
  * `EMCollisionExtraData`：同じ GameObject の Collider2D をエフェクトの HitBox として描く
  * `EMHitBoxDetector`：粒ごとの当たりの形（`GetParticleShapes`）をエフェクトの HitBox として描く
* 見た目：HurtBox は枠線だけ、HitBox（エフェクトも）は半透明の塗り（`HitBoxFillAlpha`）と枠線
* 形は Collider2D の Box・Circle・Capsule・Polygon・Composite（Edge は折れ線。それ以外は bounds の四角）と、粒の円・カプセル

## 使い方

```csharp
// 作る(Play 中だけ。__INAPPDEBUG__ が無いときは null)
var visualizer = CollisionBoxVisualizer.Show<ALCollisionBoxVisualizer>();
if (visualizer != null)
{
    visualizer.HurtBoxColor = layer => ...;   // レイヤーから色を決める(null なら DefaultColor)
    visualizer.HitBoxColor = layer => ...;
    visualizer.EffectHitBoxColor = Color.magenta;
    visualizer.VisibleKinds = ALCollisionBoxVisualizer.Kinds.HurtBox | ALCollisionBoxVisualizer.Kinds.HitBox;
}

// 消す
CollisionBoxVisualizer.Hide<ALCollisionBoxVisualizer>();
```

* 色・描く種類・塗りの不透明度・枠線の太さ（`LineWidthPixels`）・描く順（`SortingLayerID`・`SortingOrder`）は外から入れる。このクラスは設定ファイルを読まない
  * 色の設定は `UnitySpineSettings` の `Collision Box Visualizer` にある（[Spine/Folder機能.md](../Spine/Folder機能.md)）。ゲーム側が読んで渡す（`HurtBoxColor = settings.GetHurtBoxColor` など）
* ゲーム側で決めることがあれば派生クラスを作る。例：`ALCollisionExtraData` を継承した GameObject の弾を、`ClassifyUnknown` でエフェクトの HitBox にする。`Collect` を override して、集める前に設定を読み直す
* `__INAPPDEBUG__` のときだけ動く（afl の `CollisionBoxVisualizer` に合わせる）。SRP（URP など）のときだけ描かれる
* 描く対象は `RefreshInterval` 秒ごとに `FindObjectsByType` で集め直す（そのときに配列を作る）。毎フレームの処理では GC のアロケーションをしない

## 注意

* Transform から形を出して描く。物理の形は次の物理の更新で Transform に合わせられるので、Physics2D が使っている形とは、1 回の物理の更新ぶんずれることがある
* `ALHurtBoxBehaviour`・`ALHitBoxBehaviour` を使わずに付けた HurtBox・HitBox（`BoxKind` が `Unknown`）は、`ClassifyUnknown` で決めない限り描かない
