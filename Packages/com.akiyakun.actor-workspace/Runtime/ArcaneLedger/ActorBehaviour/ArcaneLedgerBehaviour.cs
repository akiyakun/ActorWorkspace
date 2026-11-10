#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;
using ActorWorkspace.ArcaneLedger;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    public struct HitResult
    {
        public static readonly HitResult NotHit = new HitResult() { IsHit = false };

        public bool IsHit;
        public float Value;
        // public Vector3 HitPoint;
        // public Vector3 HitNormal;
    }

    public class ArcaneLedgerBehaviour<TALProcessor> : ArcaneLedgerBehaviour
        where TALProcessor : class, IALProcessor, new()
    {
        public override IALProcessor Processor { get; } = new TALProcessor();
    }

    // アーケイン・レジャー
    // FIXME: 後で整理整する。雑多に処理入れてる
    // public class ArcaneLedgerBehaviour<TCalculator> : AWActorBehaviour<IAWActor>
    // where TCalculator : IALCalculator
    public abstract class ArcaneLedgerBehaviour : AWActorBehaviour<IAWActor>
    {
        public abstract IALProcessor Processor { get; }

        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Update | UpdateFlags.FixedUpdate;

        public ALGeneralParam GeneralParam { get; } = new ALGeneralParam();

        // ステータス(大きさは Processor.StatCapacity)・HP(最大 HP は Processor.MaxLifeStatId のステータス)
        // MEMO: OnAwake() で作る
        public ALStatTable Stats { get; private set; } = null!;
        public ALHealth Health { get; private set; } = null!;

        // ステータス効果が決める、本来の移動の向きの代わりに使う向き(Vector3.zero なら変えない)
        // 毎フレームの Update の始めに Vector3.zero に戻り、ステータス効果の更新で設定し直す(Id が小さい効果ほど優先)
        // MEMO: 移動の処理は ArcaneLedgerBehaviour より後で更新して、これを読む
        public Vector3 OverrideMoveDirection { get; set; }

        // 陣営(ALFaction。0〜31)と、当たる相手の陣営のビット(既定はすべての陣営に当たる)
        // MEMO: 攻撃の当たり判定(HitBox・弾)の Faction がこのアクターの HostileMask に入っていれば、このアクターに当たる
        public int Faction { get; set; }
        public uint HostileMask { get; set; } = ALFaction.All;

        MotionValueController motionValueController = null!;
        StatusEffectController statusEffectController = null!;


        public override void OnRestore()
        {
            motionValueController.Restore();
            statusEffectController.Restore();
            // MEMO: 補正を外してから HP を最大にする(最大 HP の補正が残らないように)
            // MEMO: Processor.Restore() で最大 HP の補正を付け直すとき(ソウルスキルなど)は、HP はその前の最大 HP のままなので、Processor 側で Health.Reset() し直す
            Stats.Restore();
            Health.Reset();
            OverrideMoveDirection = Vector3.zero;
            Processor.Restore();
            GeneralParam.Restore();
        }

        public override void OnAwake()
        {
            Stats = new ALStatTable(Processor.StatCapacity);
            Health = new ALHealth(Stats, Processor.MaxLifeStatId);

            motionValueController = new MotionValueController(this);
            statusEffectController = new StatusEffectController(this);

            Processor.Setup(this, statusEffectController);

            // イベント購読
            // {
            //     // AnimationController
            //     EventBag.In(Actor.AnimationController,
            //         (entity) => entity.OnAnimationEntered += OnAnimationEntered,
            //         (entity) => entity.OnAnimationEntered -= OnAnimationEntered);
            //     EventBag.In(Actor.AnimationController,
            //         (entity) => entity.OnAnimationEvent += OnAnimationEvent,
            //         (entity) => entity.OnAnimationEvent -= OnAnimationEvent);
            // }
        }

        // 購読を外す(Processor の EventBag・MotionValueController)
        public override void OnDestroy()
        {
            motionValueController?.Dispose();
            (Processor as System.IDisposable)?.Dispose();
        }

        public override void OnUpdate(float deltaTime)
        {
            OverrideMoveDirection = Vector3.zero;

            statusEffectController.Prepare();
            Processor.Prepare();

            statusEffectController.Update(deltaTime);
            Processor.Update(deltaTime);

            Health.Update(deltaTime);
        }

        public override void OnFixedUpdate(float deltaTime)
        {
            statusEffectController.FixedUpdate(deltaTime);
            Processor.FixedUpdate(deltaTime);
        }

        // public override void OnLateUpdate(float deltaTime)
        // {
        // }

        // Call from ALHurtBoxBehaviour(陣営・ヒット履歴・部位を確かめたあと)
        public void ContactWithHurtBox(in ALHitContact contact)
        {
            // me: ステータスの取得
            // me: モーション値の取得
            // other: ステータスの取得
            // other: ヒットボックスの情報から攻撃の強さや属性を取得

            var hitResult = Processor.Hit(contact);
            if (hitResult.IsHit == false) return;

            EventBus.Publish(AWCoreActorEvents.DamageReaction, Mathf.RoundToInt(hitResult.Value));

        }
    }
}
#nullable restore