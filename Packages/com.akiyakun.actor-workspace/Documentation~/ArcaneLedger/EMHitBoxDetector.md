# パーティクルの弾の当たり（EMHitBoxDetector）

パーティクル（Shuriken）の粒を弾にして、2D の HurtBox に当てるための EffectModifier です。
`Runtime/ArcaneLedger/Collider/EMHitBoxDetector.cs`

* 最終更新：2026-11-20（作った）

## なぜ Collision モジュールを使わないか

* ParticleSystem の Collision モジュール（2D World）は、トリガーのコライダーに当たらない（Physics2D の Queries Hit Triggers をオンにしても同じ）。HurtBox はトリガーなので、Collision モジュールと afl の `ShurikenCollisionEventDispatcher` では HurtBox に当たらない
* Collision モジュールはレイヤーでしか相手を絞れないので、HurtBox と同じレイヤーにある体・接地・センサーのコライダーにも当たって粒が消える
* そのため `EMHitBoxDetector` が、粒がそのフレームに進んだ線を自分で調べる

## しくみ

毎フレーム（LateUpdate）、粒ごとに「今の位置 − 速さ × そのフレームの時間」から今の位置までを、`Physics2D.CircleCast`（半径 `Radius`、トリガーも含める）で調べ、近い順に次のようにします。

| 当たったもの | どうするか |
| --- | --- |
| 地形（`TerrainLayers`。トリガーでないもの） | 粒を消す。`PassThroughEffectors` なら PlatformEffector2D の付いたもの（すり抜け床）は抜ける |
| 当たる相手（`TargetLayers`）の HurtBox | 相手のルート（Rigidbody2D のある GameObject）の `ICollisionDetector` に `ShurikenAtPosition`（当たった位置つき）で知らせる。`Pierce` でなければ粒を消す |
| それ以外 | 抜ける |

* 点ではなく線で調べるので、速い弾でも HurtBox を飛び越えない
* 出たばかりの粒は、出た位置より後ろを調べない
* 粒を消したところ・相手に当たったところで、Sub Emitters の `HitSubEmitterIndex` 番（種類は **Manual**）を出す。射程（粒の寿命）で消えたときは出ない
* 当たる相手の判定（`TryGetTarget`。virtual）の既定は、次のすべてを満たすもの
  * `IALCollisionExtraData` の付いたトリガー（HurtBox）
  * 相手のルートに `ICollisionDetector` と `IAWActor` があり、台帳（`ArcaneLedgerBehaviour`）がある
  * 相手が生きている（`ArcaneLedgerBehaviour.Health.IsAlive`）
  * 同じ GameObject の当たりの情報（`IALCollisionExtraData`）の陣営が、相手の `HostileMask` に入っている
  * 撃ったアクターが分からない弾（当たりの情報の `Actor` が無い。`EffectOption.UserData` を渡していない）は、どれにも当たらない
* 地形の判定（`IsTerrain`。virtual）の既定は、トリガーでない・（`PassThroughEffectors` なら）PlatformEffector2D に使われていないもの

## 同じ相手に何度も当たらないようにする

* 粒（`randomSeed`）と相手のルートの組を覚えて、同じ粒は同じ相手に 1 回だけ知らせる。粒が消えたら捨てる
* 貫通弾（`Pierce`）や、1 回の再生で何発も出す弾（ショットガンなど）でも、粒ごとに 1 回ずつになる
* 当たりの情報の `HitHistoryMode` は **None** にする。`ALHitResolver` は None の当たりをまとめずに 1 回ずつ当てるので、同じフレームに複数の粒が同じ相手に当たっても、粒ごとに当たる
  * PerActivation にすると、1 回の再生（同じ `SerialNumber`）の粒がまとめられ、1 回しか当たらない

## 使い方

* 弾の ParticleSystem（Simulation Space は **World**）に、`EMHitBoxDetector` と当たりの情報（`EMCollisionExtraData` の派生）を付ける
  * 当たりの情報の側に `[RequireComponent(typeof(EMHitBoxDetector))]` を付けておくと、1 つ付けるだけで済む
* 撃ったアクターは `EffectOption.UserData` で渡す（`EMCollisionExtraData` が台帳・陣営・`SerialNumber` を取る）
* 受ける側は `ALHurtBoxBehaviour`（DetectMode=Main）。知らせた相手の GameObject（弾）のレイヤーが `ALHurtBoxBehaviour.LayerMask` に入っている必要がある
* 弾の向きは、エフェクトの親の Transform などで決める（`EffectOption` には向きが無い）

## 注意

* 粒の位置・速さをワールドの値として扱う。Simulation Space が World でないと警告を出す（Editor のみ）
* DetectMode=PerHurtBox の受け手には届かない（相手のルートの `ICollisionDetector` に知らせるため）
* 当たりの威力など、当たりの情報の値は、`ALHitResolver` が確定するとき（次のフレーム）に読まれることがある。プールに返したときに値を消さないようにする
