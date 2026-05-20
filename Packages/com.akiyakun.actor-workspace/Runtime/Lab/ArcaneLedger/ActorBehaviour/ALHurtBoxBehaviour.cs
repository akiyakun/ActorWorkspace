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

            var hurtBoxFolderObject = Variables.Get(AWCoreVariableKey.HurtBoxFolder).GetGameObject();
            if (hurtBoxFolderObject != null)
            {
                var detectors = hurtBoxFolderObject.GetComponentsInChildren<ICollisionDetector>(includeInactive: true);
                for (int i = 0; i < detectors.Length; i++)
                {
                    var detector = detectors[i];

                    EventBag.In(detector,
                        (entity) => entity.OnCollision += OnCollision,
                        (entity) => entity.OnCollision -= OnCollision);
                }
            }
        }

        void OnCollision(CollisionDetectorInfo info)
        {
            Debug.Log($"ALHurtBoxBehaviour OnCollision: EventType={info.EventType}, Other={info.Other.name}");

            if (info.Other.TryGetComponent<ALHitBoxInfo>(out var hitBoxInfo))
            {
                processor.Hit(Actor, info.Other, hitBoxInfo);
            }
        }
    }
}
#nullable restore