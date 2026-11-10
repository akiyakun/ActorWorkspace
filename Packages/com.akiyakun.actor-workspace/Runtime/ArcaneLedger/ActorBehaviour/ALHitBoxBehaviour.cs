#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // 攻撃判定(HitBox)
    // HitBoxFolder の下の HitBox に ALCollisionExtraData を付け、HitBox が出るたびにシリアル番号を振り直す
    public class ALHitBoxBehaviour : AWActorBehaviour<IAWActor>
    {
        readonly List<ALCollisionExtraData> hitBoxes = new();
        ALHitHistoryMode defaultHitHistoryMode = ALHitHistoryMode.None;
        float defaultRepeatHitInterval;

        // HitBox の当たりの記録の区切り方と、同じ相手に当たり直すまでの秒数(ALCollisionExtraData の既定。最初は None・0)
        // 例: PerActivation・0 なら、HitBox が出ている間は同じ相手に 1 回だけ当たり、出直したらまた当たる
        // MEMO: Awake のあと(ActorBehaviourController.Add のあと)に呼んでよい。今ある HitBox にも入れる
        // MEMO: ALAttackBindingBehaviour(今はコメントアウトしている)は Awake のときの値を「元の値」として覚えるので、それより先に呼ぶ
        public void SetDefaultHitHistory(ALHitHistoryMode mode, float repeatHitInterval)
        {
            defaultHitHistoryMode = mode;
            defaultRepeatHitInterval = repeatHitInterval;
            foreach (var hitBox in hitBoxes) ApplyDefaultHitHistory(hitBox);
        }

        void ApplyDefaultHitHistory(ALCollisionExtraData hitBox)
        {
            hitBox.HitHistoryMode = defaultHitHistoryMode;
            hitBox.RepeatHitInterval = defaultRepeatHitInterval;
        }

        // アクターを使い回すとき、HitBox に新しい番号を振る(前に出ていたときの当たりの記録を引き継がない)
        // MEMO: 出たり消えたりしない HitBox(いつも出ている体当たりなど)は OnActivating が来ないので、ここと Awake で振る
        public override void OnRestore()
        {
            foreach (var hitBox in hitBoxes) hitBox.SerialNumber = ColliderSerialNumberGenerator.GetNext();
        }

        public override void OnAwake()
        {
            // FolderObject
            var folderObject = Variables.Get(AWCoreVariableKeys.HitBoxFolder).GetGameObject();
            if (folderObject == null)
            {
                ElementActive = false;
                return;
            }

            {
                var followers = folderObject.GetComponentsInChildren<IAWFollower>(includeInactive: true);
                for (int i = 0; i < followers.Length; i++)
                {
                    var follower = followers[i];

                    var extraInfo = follower.GameObject.GetComponent<ALCollisionExtraData>() ?? follower.GameObject.AddComponent<ALCollisionExtraData>();
                    extraInfo.Actor = Actor;
                    if (Actor.ActorBehaviourController.Get<ArcaneLedgerBehaviour>() is ArcaneLedgerBehaviour behaviour)
                    {
                        extraInfo.ALProcessor = behaviour.Processor;
                    }
                    // 番号 0 のままだと、ほかのアクターの HitBox と当たりの記録のキー(PerActivation)が同じになる
                    extraInfo.SerialNumber = ColliderSerialNumberGenerator.GetNext();
                    hitBoxes.Add(extraInfo);
                    if (defaultHitHistoryMode != ALHitHistoryMode.None) ApplyDefaultHitHistory(extraInfo);

                    // HitBox発生イベント
                    EventBag.In(follower,
                        (entity) => entity.OnActivating += OnActivatingFromFollower,
                        (entity) => entity.OnActivating -= OnActivatingFromFollower);
                }
            }
        }

        void OnActivatingFromFollower(IAWFollower follower)
        {
            ALCollisionExtraData extraInfo = follower.GameObject.GetComponent<ALCollisionExtraData>();
            if (extraInfo == null)
            {
                Debug.Assert(false, $"ALHitBoxBehaviour OnFollowerActiveChanged: ALColliderAttachmentData component not found in {follower.GameObject.name}");
                return;
            }

            // シリアル番号を振る
            extraInfo.SerialNumber = ColliderSerialNumberGenerator.GetNext();
        }
    }
}
#nullable restore