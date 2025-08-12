#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public class AWActorBehaviourController : IAWActorBehaviourController
    {
        IAWActor actor;
        UpdateElementManager<AWActorBehaviourBase> updater;

#nullable disable
        private AWActorBehaviourController() { }
#nullable enable

        public AWActorBehaviourController(IAWActor awActor)
        {
            actor = awActor;
            Debug.Assert(awActor != null);

            updater = new(enablePrioritySort: true);
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

        public virtual T Add<T>()
            where T : AWActorBehaviourBase, new()
        {
            var behaviour = AWActorBehaviourBase.Create<T>(actor);
            updater.Add(behaviour);
            return behaviour;
        }

    }
}
#nullable restore