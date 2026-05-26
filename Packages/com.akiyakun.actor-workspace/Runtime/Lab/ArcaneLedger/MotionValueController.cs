#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;

namespace ActorWorkspace.ArcaneLedger
{
    public class MotionValueController
    {
        ArcaneLedgerBehaviour owner;
        EventBag eventBag = new EventBag();

        ALGeneralParam generalParam;
        uint valueDirtyFlag;
        uint factorDirtyFlag;
        uint deltaDirtyFlag;

        public MotionValueController(ArcaneLedgerBehaviour owner)
        {
            this.owner = owner;
            generalParam = owner.GeneralParam;

            // イベント購読
            {
                eventBag.In(owner.Actor.AnimationController,
                    (entity) => entity.OnAnimationEntered += OnAnimationEntered,
                    (entity) => entity.OnAnimationEntered -= OnAnimationEntered);
                eventBag.In(owner.Actor.AnimationController,
                    (entity) => entity.OnAnimationEvent += OnAnimationEvent,
                    (entity) => entity.OnAnimationEvent -= OnAnimationEvent);
            }
        }

        public void Restore()
        {
            ResetMotionValues();

            valueDirtyFlag = 0;
            factorDirtyFlag = 0;
            deltaDirtyFlag = 0;
        }

        // 変更があったモーション値のリセット
        void ResetMotionValues()
        {
            uint value = valueDirtyFlag;
            uint factor = factorDirtyFlag;
            uint delta = deltaDirtyFlag;
            valueDirtyFlag = 0;
            factorDirtyFlag = 0;
            deltaDirtyFlag = 0;

            do
            {
                uint lsb = BitUtility.FirstSetBit(value);
                if (lsb == 0) break;
                generalParam[BitUtility.LsbToIndex(lsb)].Value = 0;
                value &= ~lsb;
            } while (value != 0);

            do
            {
                uint lsb = BitUtility.FirstSetBit(factor);
                if (lsb == 0) break;
                generalParam[BitUtility.LsbToIndex(lsb)].Factor = 0;
                factor &= ~lsb;
            } while (factor != 0);

            do
            {
                uint lsb = BitUtility.FirstSetBit(delta);
                if (lsb == 0) break;
                generalParam[BitUtility.LsbToIndex(lsb)].Delta = 0;
                delta &= ~lsb;
            } while (delta != 0);

            // for (int index = 0; index < ALGeneralParam.MaxParamCount; index++)
            // {
            //     uint mask = 1u << index;
            //     if ((value & mask) != 0)
            //     {
            //         generalParam[index].Value = 0;
            //     }
            //     if ((factor & mask) != 0)
            //     {
            //         generalParam[index].Factor = 0;
            //     }
            //     if ((delta & mask) != 0)
            //     {
            //         generalParam[index].Delta = 0;
            //     }
            // }
        }

        void OnAnimationEntered(IAWAnimation animation)
        {
            ResetMotionValues();
        }

        void OnAnimationEvent(IAWAnimation animation, AWAnimationEventData eventData)
        {
            // 汎用パラメータ設定イベントの場合
            if (eventData.Name.StartsWith(AWCoreAnimationEvents.SetGenPrefix, System.StringComparison.Ordinal))
            {
                ProcessSetGenEvent(animation, ref eventData);
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

        void ProcessSetGenEvent(IAWAnimation animation, ref AWAnimationEventData eventData)
        {
            // SetGenValue[X] イベント
            if (eventData.Name.StartsWith(AWCoreAnimationEvents.SetGenValuePrefix, System.StringComparison.Ordinal))
            {
                int index = eventData.Name[AWCoreAnimationEvents.SetGenValuePrefix.Length] - 'A';
                if (index < 0 || index >= AWCoreAnimationEvents.MaxGenParamCount) throw new System.Exception($"Invalid SetGenValue event name: {eventData.Name}");
                generalParam[index].Value = eventData.Float;
                D.Log(CoreLogMask.Events, $"SetGenValue: {(char)('A' + index)}, value={eventData.Float}");
                valueDirtyFlag |= (uint)(1u << index);
            }
            // SetGenFactor[X] イベント
            else if (eventData.Name.StartsWith(AWCoreAnimationEvents.SetGenFactorPrefix, System.StringComparison.Ordinal))
            {
                int index = eventData.Name[AWCoreAnimationEvents.SetGenFactorPrefix.Length] - 'A';
                if (index < 0 || index >= AWCoreAnimationEvents.MaxGenParamCount) throw new System.Exception($"Invalid SetGenFactor event name: {eventData.Name}");
                generalParam[index].Factor = eventData.Float;
                D.Log(CoreLogMask.Events, $"SetGenFactor: {(char)('A' + index)}, factor={eventData.Float}");
                factorDirtyFlag |= (uint)(1u << index);
            }
            // SetGenDelta[X] イベント
            else if (eventData.Name.StartsWith(AWCoreAnimationEvents.SetGenDeltaPrefix, System.StringComparison.Ordinal))
            {
                int index = eventData.Name[AWCoreAnimationEvents.SetGenDeltaPrefix.Length] - 'A';
                if (index < 0 || index >= AWCoreAnimationEvents.MaxGenParamCount) throw new System.Exception($"Invalid SetGenDelta event name: {eventData.Name}");
                generalParam[index].Delta = eventData.Float;
                D.Log(CoreLogMask.Events, $"SetGenDelta: {(char)('A' + index)}, delta={eventData.Float}");
                deltaDirtyFlag |= (uint)(1u << index);
            }
            else
            {
                Debug.Assert(false, $"Unknown SetGen event: {eventData.Name}");
            }
        }

    }
}
#nullable restore