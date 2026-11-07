[TOC]

# ActorBehaviour

ActorBehaviourフォルダーにはライブラリ側で用意されたActorBehaviour実装クラスが入っています。

名前の衝突を避けるためにクラス名にプレフィックスとして`AW`を使用しています。

## AWBulletBehaviour

戦闘に関する基本的な振る舞いを提供します。

## IBattleTargetSelector

戦闘のターゲットを選択するインターフェース

EnemyTargetSelector や PlayerTargetSelector を実装するイメージ

## AWInteractEventPublisherBehaviour

汎用的なインタラクトイベント発行をする振る舞いを提供します。

## AWAudioEventBehaviour

モーションの `Audio` イベントで SE を鳴らします（2026-11-07 追加）。

* `AWCoreAnimationEventBehaviour` が、モーションの `Audio` イベント（Int 値が SE マスタの Id）を `AWCoreActorEvents.Audio` に流し、これが受けて鳴らす
* 鳴らすのは afl の `SEContext`（`afl.GameAudio`）。アクターの位置を渡すので、画面の外なら鳴らさず、画面の左右の位置でパンする。`SEContext` は初めて鳴らすときに `UniversalContextManager` から取る（登録しておく）
* SE マスタの `StopOnMotionInterrupt` が 1 の SE は、鳴らしたモーションを覚えておき（afl の `SEMotionInterruptTracker`）、別のモーションが始まったときと、アクターが消えたとき（`AWCoreActorEvents.Despawn`・`StopMotionSE`・Behaviour の破棄・使い回し）にまだ鳴っていれば止める
* アクターのマネージャーの `Despawn()` は Behaviour に知らせないので、直接 Despawn するコードは先に `AWCoreActorEvents.StopMotionSE` を発行する
* 鳴らし方を変えたいときは、派生クラスで `OnAudio(int id)` を override する
* BGM・SE の仕組みは afl の `docs~/Services/Audio/_Index_Audio.md` の「BGM・SE のマスタと再生」



