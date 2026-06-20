#nullable enable
using UnityEngine;
using afl;
using afl.Service.Input;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    public class ALHitBoxBehaviour : AWActorBehaviour<IAWActor>
    {
        // FIXME: ここじゃない
        static uint currentSerialNumber = 0;
        public static uint GetNextSerialNumber()
        {
            return unchecked(++currentSerialNumber);
        }

        public override void OnRestore()
        {

        }

        public override void OnAwake()
        {
            // CollisionDetector
            // {
            //     var collisionDetector = Variables.Get(AWCoreVariableKeys.MainCollisionDetector).GetComponent<CollisionDetector>();
            //     if (collisionDetector == null) throw new System.Exception($"ALHitBoxBehaviour DoAwake: CollisionDetector not found in variable {AWCoreVariableKeys.MainCollisionDetector}");
            //     EventBag.In(collisionDetector,
            //         (entity) => entity.OnCollision += OnCollision,
            //         (entity) => entity.OnCollision -= OnCollision);
            // }

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

                    var extraInfo = follower.GameObject.AddComponent<ALCollisionExtraData>();
                    extraInfo.Actor = Actor;
                    if (Actor.ActorBehaviourController.Get<ArcaneLedgerBehaviour>() is ArcaneLedgerBehaviour behaviour)
                    {
                        extraInfo.ALProcessor = behaviour.Processor;
                    }

                    // HitBox発生イベント
                    EventBag.In(follower,
                        (entity) => entity.OnActivating += OnActivatingFromFollower,
                        (entity) => entity.OnActivating -= OnActivatingFromFollower);
                }
            }
        }

        void OnActivatingFromFollower(IAWFollower follower)
        {
            // Debug.Log($"ALHitBoxBehaviour OnFollowerActiveChanged: {follower}");

            ALCollisionExtraData extraInfo = follower.GameObject.GetComponent<ALCollisionExtraData>();
            if (extraInfo == null)
            {
                Debug.Assert(false, $"ALHitBoxBehaviour OnFollowerActiveChanged: ALColliderAttachmentData component not found in {follower.GameObject.name}");
                return;
            }

            // シリアル番号を振る
            extraInfo.SerialNumber = GetNextSerialNumber();
            // Debug.Log($"{currentSerialNumber}");

        }
    }
}
#nullable restore