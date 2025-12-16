#nullable enable
using UnityEngine;
using afl;
using ActorWorkspace.Common;

namespace ActorWorkspace.ActorBehaviour
{
    public class InteractEventPublisherBehaviour : AWActorBehaviour<IAWActor>
    {

        public override void Restore()
        {
        }

        public override void DoAwake()
        {
            if (Variables.TryGet(AWVariableKey.Hurtbox1, out var hurtbox1))
            {
                GameObject obj = hurtbox1.Get<GameObject>();
                if (obj == null) throw new System.Exception("Hurtbox1(GameObject) is null");

                var detector = obj.GetComponent<ICollisionDetector>();
                if (detector == null) throw new System.Exception("Hurtbox1(GameObject) does not have ICollisionDetector component");

                EventBag.In(detector,
                    (entity) => entity.OnCollision += OnHurtbox1Enter,
                    (entity) => entity.OnCollision -= OnHurtbox1Enter);

                // Debug.Log($"InteractEventPublisherBehaviour: Registered OnHurtbox1Enter");
            }
        }

        void OnHurtbox1Enter(CollisionDetectorInfo info)
        {
            // Debug.Log($"InteractEventPublisherBehaviour: OnHurtbox1Enter: other={info.Other?.name}");
            EventBus.Publish(AWActorEvents.OnInteractHurtbox1, info);
        }
    }
}
#nullable restore