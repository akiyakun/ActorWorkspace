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

        protected Dictionary<string, System.Action> eventActions = new();

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
                // eventActions.Add(AWCoreAnimationEvents.Effect, () => OnEffectEvent());
            }
        }

        protected void AddEventAction(string eventName, System.Action action)
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
                action?.Invoke();
            }
            else
            {
                Debug.Assert(false, $"Unknown animation event: {eventData.Name}");
            }

            // switch (eventData.Name)
            // {
            //     // AWCoreAnimationEvents

            //     case AWCoreAnimationEvents.MotionConfig:
            //         EventBus.Publish(ActorEvents.MotionConfig, eventData.Int);
            //         break;
            //     case AWCoreAnimationEvents.Audio:
            //         EventBus.Publish(ActorEvents.Audio, eventData.Int);
            //         break;
            //     default:
            //         Debug.Assert(false, $"Unknown animation event: {eventData.Name}");
            //         break;
            // }
        }
    }
}
#nullable restore