#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // FIXME: 後で整理整する。雑多に処理入れてる
    public class ArcaneLedgerBehaviour : AWActorBehaviour<IAWActor>
    {
        class GeneralParam
        {
            public float Value;
            public float Factor;
            public float Modifier;

            public GeneralParam()
            {
                Restore();
            }

            public void Restore()
            {
                Value = 0.0f;
                Factor = 1.0f;
                Modifier = 0.0f;
            }
        }
        GeneralParam[] generalParams = new GeneralParam[AWCoreAnimationEvents.MaxGenParamCount];

        public override void Restore()
        {
            for (int i = 0; i < generalParams.Length; i++)
            {
                generalParams[i].Restore();
            }
        }

        public override void DoAwake()
        {
            for (int i = 0; i < generalParams.Length; i++)
            {
                generalParams[i] = new GeneralParam();
            }

            // イベント購読
            {
                EventBag.In(Actor.AnimationController,
                    (entity) => entity.OnAnimationEvent += OnAnimationEvent,
                    (entity) => entity.OnAnimationEvent -= OnAnimationEvent);
            }
        }

        public void Hit(IAWActor me, GameObject other, ALHitBoxInfo hitBoxInfo)
        {

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

        void ProcessSetGenEvent(IAWAnimation animation, AWAnimationEventData eventData)
        {
            // SetGenValue[X] イベント
            if (eventData.Name.StartsWith(AWCoreAnimationEvents.SetGenValuePrefix, System.StringComparison.Ordinal))
            {
                int index = eventData.Name[AWCoreAnimationEvents.SetGenValuePrefix.Length] - 'A';
                if (index < 0 || index >= AWCoreAnimationEvents.MaxGenParamCount) throw new System.Exception($"Invalid SetGenValue event name: {eventData.Name}");
                generalParams[index].Value = eventData.Float;
                Debug.Log($"SetGenValue: {(char)('A' + index)}, value={eventData.Float}");
            }
            // SetGenFactor[X] イベント
            else if (eventData.Name.StartsWith(AWCoreAnimationEvents.SetGenFactorPrefix, System.StringComparison.Ordinal))
            {
                int index = eventData.Name[AWCoreAnimationEvents.SetGenFactorPrefix.Length] - 'A';
                if (index < 0 || index >= AWCoreAnimationEvents.MaxGenParamCount) throw new System.Exception($"Invalid SetGenFactor event name: {eventData.Name}");
                generalParams[index].Factor = eventData.Float;
                Debug.Log($"SetGenFactor: {(char)('A' + index)}, factor={eventData.Float}");
            }
            // SetGenModifier[X] イベント
            else if (eventData.Name.StartsWith(AWCoreAnimationEvents.SetGenModifierPrefix, System.StringComparison.Ordinal))
            {
                int index = eventData.Name[AWCoreAnimationEvents.SetGenModifierPrefix.Length] - 'A';
                if (index < 0 || index >= AWCoreAnimationEvents.MaxGenParamCount) throw new System.Exception($"Invalid SetGenModifier event name: {eventData.Name}");
                generalParams[index].Modifier = eventData.Float;
                Debug.Log($"SetGenModifier: {(char)('A' + index)}, modifier={eventData.Float}");
            }
            else
            {
                Debug.Assert(false, $"Unknown SetGen event: {eventData.Name}");
            }
        }
    }
}
#nullable restore