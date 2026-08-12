#nullable enable
using UnityEngine;
using afl;
using afl.Service.Effects;
using System.Collections.Generic;

namespace ActorWorkspace.ActorBehaviour
{
    // AnimationEventをActorEventsに通知する
    public class AWCoreAnimationEventBehaviour<TActor> : AWActorBehaviour<TActor>
        where TActor : class, IAWActor
    {
        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Manual;

        protected Dictionary<string, System.Action<AWAnimationEventData>> eventActions = new();

        public override void OnRestore()
        {
        }

        public override void OnAwake()
        {
            // Events
            {
                EventBag.In(Actor.AnimationController,
                    (entity) => entity.OnAnimationEvent += OnAnimationEvent,
                    (entity) => entity.OnAnimationEvent -= OnAnimationEvent);
            }

            // Adds event actions
            {
                // AddEventAction(AWCoreAnimationEvents.Effect, (eventData) => OnEffectEvent());
            }
        }

        protected void AddEventAction(string eventName, System.Action<AWAnimationEventData> action)
        {
            eventActions.Add(eventName, action);
        }

        protected virtual void OnAnimationEvent(IAWAnimation animation, AWAnimationEventData eventData)
        {
            // Effect events
            if (eventData.Name.StartsWith(AWCoreAnimationEvents.Effect, System.StringComparison.Ordinal))
            {
                EventBus.Publish(AWCoreActorEvents.Effect, new AWCoreActorEvents.EffectInfo
                {
                    Name = eventData.Name,
                    Id = eventData.Int,
                });
                return;
            }

            if (eventActions.TryGetValue(eventData.Name, out var action))
            {
                action?.Invoke(eventData);
            }
            else
            {
                Debug.Assert(false, $"Unknown animation event: {eventData.Name}");
            }

        }
    }
}
#nullable restore