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

        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Update;

        public ALGeneralParam GeneralParam { get; } = new ALGeneralParam();

        MotionValueController motionValueController = null!;
        StatusEffectController statusEffectController = null!;


        public override void Restore()
        {
            motionValueController.Restore();
            statusEffectController.Restore();
            Processor.Restore();
            GeneralParam.Restore();
        }

        public override void DoAwake()
        {
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

        public override void DoUpdate(float deltaTime)
        {
            statusEffectController.Prepare();
            Processor.Prepare();

            statusEffectController.Update(deltaTime);
            Processor.Update(deltaTime);
        }

        public override void DoLateUpdate(float deltaTime)
        {

        }

        public void ContactWithHurtBox(CollisionContactInfo contactInfo)
        {
            // me: ステータスの取得
            // me: モーション値の取得
            // other: ステータスの取得
            // other: ヒットボックスの情報から攻撃の強さや属性を取得

            var hitResult = Processor.Hit(contactInfo);
            if (hitResult.IsHit == false) return;

            EventBus.Publish(AWCoreActorEvents.DamageReaction, Mathf.RoundToInt(hitResult.Value));

        }
    }
}
#nullable restore