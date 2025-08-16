#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public class AWActorBehaviourController : IAWActorBehaviourController
    {
        public event System.Action<AWActorBehaviour>? OnBehaviourAdded;
        public event System.Action<AWActorBehaviour>? OnBehaviourRemoved;

        IAWActor actor;
        UpdateElementManager<AWActorBehaviour> updater;

#nullable disable
        private AWActorBehaviourController() { }
#nullable enable

        public AWActorBehaviourController(IAWActor awActor)
        {
            actor = awActor;
            Debug.Assert(awActor != null);

            updater = new(enablePrioritySort: true, enforceUniqueType: true);
        }

        public virtual void DoUpdate(float deltaTime)
        {
            updater.DoUpdate(deltaTime);
        }

        public virtual void DoLateUpdate(float deltaTime)
        {
            updater.DoLateUpdate(deltaTime);
        }

        public virtual void DoFixedUpdate()
        {
            updater.DoFixedUpdate(/*deltaTime*/);// FIXME
        }

        public virtual T? Add<T>()
            where T : AWActorBehaviour, new()
        {
            var behaviour = AWActorBehaviour.Create<T>(actor);
            if (updater.Add(behaviour) == false) return null;
            behaviour.DoAwake();
            OnBehaviourAdded?.Invoke(behaviour);
            return behaviour;
        }

        public virtual T? Remove<T>()
            where T : AWActorBehaviour
        {
            var behaviour = updater.Remove<T>();
            if (behaviour == null) return null;
            OnBehaviourRemoved?.Invoke(behaviour);
            return behaviour;
        }

    }
}
#nullable restore