#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public class AWActorBehaviourController
    {
        public event System.Action<IAWActorBehaviour> OnBehaviourAdded = null!;
        public event System.Action<IAWActorBehaviour> OnBehaviourRemoved = null!;

        IAWActor actor;
        UpdateElementManager<IAWActorBehaviour> updater;

#nullable disable
        private AWActorBehaviourController() { }
#nullable enable

        public AWActorBehaviourController(IAWActor awActor)
        {
            actor = awActor;
            Debug.Assert(awActor != null);

            updater = new(enablePrioritySort: true, enforceUniqueType: true);
            updater.OnRemoveElement += OnRemoveBehaviour;
        }

        public virtual void Dispose()
        {
            if (updater != null)
            {
                updater.ReleaseAll();
                updater.OnRemoveElement -= OnRemoveBehaviour;
                updater = null!;
            }
        }

        public virtual void Restore()
        {
            var list = updater.ReadOnlyList;
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                list[i].Restore();
            }
        }

        public virtual void DoUpdate(float deltaTime)
        {
            updater.DoUpdate(deltaTime);
        }

        public virtual void DoLateUpdate(float deltaTime)
        {
            updater.DoLateUpdate(deltaTime);
        }

        public virtual void DoFixedUpdate(float deltaTime)
        {
            updater.DoFixedUpdate(deltaTime);
        }

        public virtual T? Add<T>()
            where T : class, IAWActorBehaviour, new()
        {
            // var behaviour = IAWActorBehaviour.Create<T>(actor);
            var behaviour = new T();
            behaviour.Initialize(actor);

            if (updater.Add(behaviour) == false) return null;
            behaviour.Awake();
            OnBehaviourAdded?.Invoke(behaviour);
            return behaviour;
        }

        public virtual bool Remove<T>()
            where T : class, IAWActorBehaviour
        {
            var ret = updater.RemoveImmediate<T>();
            return ret;
        }

        void OnRemoveBehaviour(IAWActorBehaviour behaviour)
        {
            OnBehaviourRemoved?.Invoke(behaviour);
            behaviour.Destroy();
            behaviour.Terminate();
        }

        public virtual T? Get<T>()
            where T : class, IAWActorBehaviour
        {
            return updater.Get<T>();
        }

    }
}
#nullable restore