#nullable enable
using UnityEngine;
using afl;
using afl.Service.Effects;

namespace ActorWorkspace.ActorBehaviour
{
    public class AWEffectEventBehaviour : AWActorBehaviour<IAWActor>
    {
        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Manual;

        public override void OnRestore()
        {
        }

        public override void OnAwake()
        {
            // Events
            {
                EventBag.In(EventBus,
                    (entity) => entity.Subscribe(AWCoreActorEvents.Effect, OnEffect),
                    (entity) => entity.Unsubscribe(AWCoreActorEvents.Effect, OnEffect));
            }
        }

        protected virtual void OnEffect(AWCoreActorEvents.EffectInfo effectInfo)
        {
            // Debug.Log($"AWEffectEventBehaviour: OnEffect() Name={effectInfo.Name} Id={effectInfo.Id}");

            var obj = Actor.AnimationController.GetFollowObject(effectInfo.Name);
            if (obj == null) throw new System.Exception($"AWEffectEventBehaviour: OnEffect() Name={effectInfo.Name} not found follow object.");

            var option = new EffectOption
            {
                Parent = obj.transform,
                // Position = Vector3.zero,
                // Rotation = Quaternion.identity,
                Flags = CoreEffectOptionFlags.Actor,
                UserData = Actor,
            };
            EffectManager.Instance.Play(effectInfo.Id, option: option);
        }
    }
}
#nullable restore