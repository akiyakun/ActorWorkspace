#nullable enable
using UnityEngine;
using afl;
using afl.Service.Input;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    public class ALHurtBoxBehaviour : AWActorBehaviour<IAWActor>
    {
        ArcaneLedgerBehaviour processor = null!;

        public override void Restore()
        {

        }

        public override void DoAwake()
        {
            processor = Actor.ActorBehaviourController.Get<ArcaneLedgerBehaviour>()!;
            if (processor == null) throw new System.Exception("ALHurtBoxBehaviour DoAwake: ArcaneLedgerBehaviour not found in ActorBehaviourController");

            var folderObject = Variables.Get(AWCoreVariableKey.HurtBoxFolder).GetGameObject();
            if (folderObject != null)
            {
                var followers = folderObject.GetComponentsInChildren<IAWFollower>(includeInactive: true);
                for (int i = 0; i < followers.Length; i++)
                {
                    var follower = followers[i];

                    var extraInfo = follower.GameObject.AddComponent<ALColliderExtraInfo>();
                    extraInfo.Actor = Actor;

                    var detector = follower.GameObject.GetComponent<ICollisionDetector>();
                    EventBag.In(detector,
                        (entity) => entity.OnCollision += OnCollision,
                        (entity) => entity.OnCollision -= OnCollision);
                }
            }
        }

        void OnCollision(CollisionContactInfo info)
        {
            // Debug.Log($"ALHurtBoxBehaviour OnCollision: EventType={info.EventType}, Other={info.Other.name}");
            processor.ContactWithHurtBox(info);
        }
    }
}
#nullable restore