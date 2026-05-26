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

    // アーケイン・レジャー
    // FIXME: 後で整理整する。雑多に処理入れてる
    // public class ArcaneLedgerBehaviour<TCalculator> : AWActorBehaviour<IAWActor>
    // where TCalculator : IALCalculator
    public class ArcaneLedgerBehaviour : AWActorBehaviour<IAWActor>
    {
        ActorWorkspace.ArcaneLedger.DefaultALProcessor processor = null!;
        public ActorWorkspace.ArcaneLedger.DefaultALProcessor Processor => processor;

        uint animationGenParamValueDirtyFlag = 0;
        uint animationGenParamFactorDirtyFlag = 0;
        uint animationGenParamModifierDirtyFlag = 0;

        StatusEffectController statusEffectController = null!;

        public override void Restore()
        {
            ResetMotionValues();
        }

        public override void DoAwake()
        {
            processor = new DefaultALProcessor(Actor);
            statusEffectController = new StatusEffectController(Actor, processor);

            // イベント購読
            {
                // AnimationController
                EventBag.In(Actor.AnimationController,
                    (entity) => entity.OnAnimationEntered += OnAnimationEntered,
                    (entity) => entity.OnAnimationEntered -= OnAnimationEntered);
                EventBag.In(Actor.AnimationController,
                    (entity) => entity.OnAnimationEvent += OnAnimationEvent,
                    (entity) => entity.OnAnimationEvent -= OnAnimationEvent);
            }
        }

        public override void DoUpdate(float deltaTime)
        {
            statusEffectController.DoPrepare();
            statusEffectController.DoUpdate(deltaTime);
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

            var hitResult = processor.Hit(contactInfo);
            if (hitResult.IsHit == false) return;

            EventBus.Publish(AWCoreActorEvents.DamageReaction, Mathf.RoundToInt(hitResult.Value));

        }

        protected virtual void OnAnimationEntered(IAWAnimation animation)
        {
            ResetMotionValues();
        }

        void OnAnimationEvent(IAWAnimation animation, AWAnimationEventData eventData)
        {
            // 汎用パラメータ設定イベントの場合
            if (eventData.Name.StartsWith(AWCoreAnimationEvents.SetGenPrefix, System.StringComparison.Ordinal))
            {
                ProcessSetGenEvent(animation, eventData);
                return;
            }

            // switch (eventData.Name)
            // {
            //     case AWCoreAnimationEvents.MotionConfig:
            //         // EventBus.Publish(ActorEvents.MotionConfig, eventData.Int);
            //         break;
            //     default:
            //         Debug.Assert(false, $"Unknown animation event: {eventData.Name}");
            //         break;
            // }
        }

        // モーション値のリセット
        void ResetMotionValues()
        {
            uint valueDirtyFlag = animationGenParamValueDirtyFlag;
            uint factorDirtyFlag = animationGenParamFactorDirtyFlag;
            uint modifierDirtyFlag = animationGenParamModifierDirtyFlag;

            animationGenParamValueDirtyFlag = 0;
            animationGenParamFactorDirtyFlag = 0;
            animationGenParamModifierDirtyFlag = 0;

            for (int index = 0; index < AWCoreAnimationEvents.MaxGenParamCount; index++)
            {
                if ((valueDirtyFlag & (1u << index)) != 0)
                {
                    Processor.GeneralParams[index].Value = 0;
                }
                if ((factorDirtyFlag & (1u << index)) != 0)
                {
                    Processor.GeneralParams[index].Factor = 0;
                }
                if ((modifierDirtyFlag & (1u << index)) != 0)
                {
                    Processor.GeneralParams[index].Modifier = 0;
                }
            }
        }

        void ProcessSetGenEvent(IAWAnimation animation, AWAnimationEventData eventData)
        {
            // SetGenValue[X] イベント
            if (eventData.Name.StartsWith(AWCoreAnimationEvents.SetGenValuePrefix, System.StringComparison.Ordinal))
            {
                int index = eventData.Name[AWCoreAnimationEvents.SetGenValuePrefix.Length] - 'A';
                if (index < 0 || index >= AWCoreAnimationEvents.MaxGenParamCount) throw new System.Exception($"Invalid SetGenValue event name: {eventData.Name}");
                Processor.GeneralParams[index].Value = eventData.Float;
                D.Log(CoreLogMask.Events, $"SetGenValue: {(char)('A' + index)}, value={eventData.Float}");
                animationGenParamValueDirtyFlag |= (uint)(1u << index);
            }
            // SetGenFactor[X] イベント
            else if (eventData.Name.StartsWith(AWCoreAnimationEvents.SetGenFactorPrefix, System.StringComparison.Ordinal))
            {
                int index = eventData.Name[AWCoreAnimationEvents.SetGenFactorPrefix.Length] - 'A';
                if (index < 0 || index >= AWCoreAnimationEvents.MaxGenParamCount) throw new System.Exception($"Invalid SetGenFactor event name: {eventData.Name}");
                Processor.GeneralParams[index].Factor = eventData.Float;
                D.Log(CoreLogMask.Events, $"SetGenFactor: {(char)('A' + index)}, factor={eventData.Float}");
                animationGenParamFactorDirtyFlag |= (uint)(1u << index);
            }
            // SetGenModifier[X] イベント
            else if (eventData.Name.StartsWith(AWCoreAnimationEvents.SetGenModifierPrefix, System.StringComparison.Ordinal))
            {
                int index = eventData.Name[AWCoreAnimationEvents.SetGenModifierPrefix.Length] - 'A';
                if (index < 0 || index >= AWCoreAnimationEvents.MaxGenParamCount) throw new System.Exception($"Invalid SetGenModifier event name: {eventData.Name}");
                Processor.GeneralParams[index].Modifier = eventData.Float;
                D.Log(CoreLogMask.Events, $"SetGenModifier: {(char)('A' + index)}, modifier={eventData.Float}");
                animationGenParamModifierDirtyFlag |= (uint)(1u << index);
            }
            else
            {
                Debug.Assert(false, $"Unknown SetGen event: {eventData.Name}");
            }
        }
    }
}
#nullable restore