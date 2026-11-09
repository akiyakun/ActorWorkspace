#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // アニメーションイベント SetAttack(int: 攻撃の番号)で、HitBox に攻撃を持たせる
    // 攻撃の番号から、アニメーションの追加情報(IAWHitAreaInfoProvider)の対応表(AWAttackBinding)を引き、
    // 名前の合う HitBox の ALCollisionExtraData に AttackId・RepeatHitInterval を入れ、HitHistoryMode を PerActivation にする
    // 次のアニメーションに入ると、元に戻す
    //
    // MEMO: HitBox の ALCollisionExtraData は ALHitBoxBehaviour が付ける(無ければここで付ける)
    public class ALAttackBindingBehaviour : AWActorBehaviour<IAWActor>
    {
        struct HitBoxEntry
        {
            public ALCollisionExtraData ExtraData;
            public int DefaultAttackId;
            public ALHitHistoryMode DefaultHitHistoryMode;
            public float DefaultRepeatHitInterval;
        }

        readonly List<HitBoxEntry> hitBoxes = new();
        IAWHitAreaInfoProvider? hitAreaInfo;

        public override void OnRestore()
        {
            ResetAttack();
        }

        public override void OnAwake()
        {
            hitAreaInfo = Actor.AnimationController?.ExtraData as IAWHitAreaInfoProvider;

            var folderObject = Variables.Get(AWCoreVariableKeys.HitBoxFolder).GetGameObject();
            if (folderObject == null || Actor.AnimationController == null)
            {
                ElementActive = false;
                return;
            }

            var followers = folderObject.GetComponentsInChildren<IAWFollower>(includeInactive: true);
            foreach (var follower in followers)
            {
                var extraData = follower.GameObject.GetComponent<ALCollisionExtraData>() ?? follower.GameObject.AddComponent<ALCollisionExtraData>();
                if (extraData.Actor == null) extraData.Actor = Actor;
                hitBoxes.Add(new HitBoxEntry()
                {
                    ExtraData = extraData,
                    DefaultAttackId = extraData.AttackId,
                    DefaultHitHistoryMode = extraData.HitHistoryMode,
                    DefaultRepeatHitInterval = extraData.RepeatHitInterval,
                });
            }

            EventBag.In(Actor.AnimationController,
                (entity) => entity.OnAnimationEntered += OnAnimationEntered,
                (entity) => entity.OnAnimationEntered -= OnAnimationEntered);
            EventBag.In(Actor.AnimationController,
                (entity) => entity.OnAnimationEvent += OnAnimationEvent,
                (entity) => entity.OnAnimationEvent -= OnAnimationEvent);
        }

        void OnAnimationEntered(IAWAnimation animation)
        {
            ResetAttack();
        }

        void OnAnimationEvent(IAWAnimation animation, AWAnimationEventData eventData)
        {
            if (eventData.Name != AWCoreAnimationEvents.SetAttack) return;
            SetAttack(eventData.Int);
        }

        // 攻撃の番号の攻撃を HitBox に持たせる
        public void SetAttack(int localId)
        {
            if (hitAreaInfo == null || hitAreaInfo.TryGetAttackBinding(localId, out var binding) == false)
            {
                Debug.LogWarning($"ALAttackBindingBehaviour: 攻撃の対応表に無い番号です。localId={localId}, actor={Actor.GameObject?.name}");
                return;
            }

            foreach (var entry in hitBoxes)
            {
                if (IsTarget(binding, entry.ExtraData.gameObject.name) == false) continue;
                entry.ExtraData.AttackId = binding.AttackId;
                entry.ExtraData.RepeatHitInterval = binding.RepeatHitInterval;
                entry.ExtraData.HitHistoryMode = ALHitHistoryMode.PerActivation;
            }
        }

        // HitBox を元に戻す(攻撃を持たない)
        public void ResetAttack()
        {
            foreach (var entry in hitBoxes)
            {
                entry.ExtraData.AttackId = entry.DefaultAttackId;
                entry.ExtraData.HitHistoryMode = entry.DefaultHitHistoryMode;
                entry.ExtraData.RepeatHitInterval = entry.DefaultRepeatHitInterval;
            }
        }

        static bool IsTarget(in AWAttackBinding binding, string hitBoxName)
        {
            if (binding.HitBoxNames == null || binding.HitBoxNames.Length == 0) return true;
            foreach (var name in binding.HitBoxNames)
            {
                if (name == hitBoxName) return true;
            }
            return false;
        }
    }
}
#nullable restore
