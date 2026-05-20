#nullable enable
using UnityEngine;
using afl;
using afl.Service.Input;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    public class ALHurtBoxBehaviour : AWActorBehaviour<IAWActor>
    {
        public override void Restore()
        {

        }

        public override void DoAwake()
        {
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
        }
    }
}
#nullable restore