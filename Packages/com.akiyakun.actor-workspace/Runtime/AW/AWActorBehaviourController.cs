#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public class AWActorBehaviourController : IAWActorBehaviourController
    {
        public event System.Action<AWActorBehaviour> OnBehaviourAdded = null!;
        public event System.Action<AWActorBehaviour> OnBehaviourRemoved = null!;

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
            updater.OnRemoveElement += OnRemoveBehaviour;
        }

        public void Dispose()
        {
            updater.OnRemoveElement -= OnRemoveBehaviour;
        }

        public virtual void Restore()
        {
            var list = updater.ReadOnlyList;
            for (int i = 0; i < list.Count; i++)
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

        public virtual bool Remove<T>()
            where T : AWActorBehaviour
        {
            var ret = updater.RemoveImmediate<T>();
            return ret;
        }

        void OnRemoveBehaviour(AWActorBehaviour behaviour)
        {
            OnBehaviourRemoved?.Invoke(behaviour);
            behaviour.DoDestroy();
        }

        public virtual T? Get<T>()
            where T : AWActorBehaviour
        {
            return updater.Get<T>();
        }

    }
}
#nullable restore