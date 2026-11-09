#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // HurtBoxはHitBoxとのみ接触する想定
    // IALColliderAttachmentData が取得できる前提
    public class ALHurtBoxBehaviour : AWActorBehaviour<IAWActor>
    {
        public int LayerMask { get; set; }

        ArcaneLedgerBehaviour processor = null!;

        public override void OnRestore()
        {

        }

        public override void OnAwake()
        {
            processor = Actor.ActorBehaviourController.Get<ArcaneLedgerBehaviour>()!;
            if (processor == null) throw new System.Exception("ALHurtBoxBehaviour DoAwake: ArcaneLedgerBehaviour not found in ActorBehaviourController");

            // CollisionDetector
            {
                var collisionDetector = Variables.Get(AWCoreVariableKeys.MainCollisionDetector).GetComponent<CollisionDetector>();
                if (collisionDetector == null) throw new System.Exception($"ALHurtBoxBehaviour DoAwake: CollisionDetector not found in variable {AWCoreVariableKeys.MainCollisionDetector}");
                EventBag.In(collisionDetector,
                    (entity) => entity.OnCollision += OnCollision,
                    (entity) => entity.OnCollision -= OnCollision);
            }

            // FolderObject
            var folderObject = Variables.Get(AWCoreVariableKeys.HurtBoxFolder).GetGameObject();
            if (folderObject == null)
            {
                ElementActive = false;
                return;
            }

            {
                // GetComponentsInChildren()は自身も含む
                var followers = folderObject.GetComponentsInChildren<IAWFollower>(includeInactive: true);
                int count = followers.Length;
                for (int i = 0; i < count; i++)
                {
                    var follower = followers[i];

                    var extraInfo = follower.GameObject.AddComponent<ALCollisionExtraData>();
                    extraInfo.Actor = Actor;
                    // if (Actor.ActorBehaviourController.Get<ArcaneLedgerBehaviour>() is ArcaneLedgerBehaviour behaviour)
                    {
                        extraInfo.ALProcessor = processor.Processor;
                    }

                    // var detector = follower.GameObject.GetComponent<ICollisionDetector>();
                    // EventBag.In(detector,
                    //     (entity) => entity.OnCollision += OnCollision,
                    //     (entity) => entity.OnCollision -= OnCollision);
                }
            }
        }

        void OnCollision(CollisionContactInfo info)
        {
            // D.Log($"ALHurtBoxBehaviour OnCollision: EventType={info.EventType}\nOther: {info.Other.GetHierarchyPath()}", ColorEx.LightBlue);
            // int o = info.Other.gameObject.layer;
            // 対象のレイヤー以外なら抜ける
            if (info.Other.HasLayerFromMask(LayerMask) == false) return;

            D.Log($"ALHurtBoxBehaviour OnCollision: EventType={info.EventType}\nOther: {info.Other.GetHierarchyPath()}", ColorEx.LightBlue);

#if UNITY_EDITOR
            var extraData = info.Other.GetComponent<EMCollisionExtraData>();
            if (extraData != null && extraData.DebugBreakPause)
            {
                D.DebugEditorPause();
                D.DebugBreak();
            }
#endif

            processor.ContactWithHurtBox(info);
        }
    }
}
#nullable restore