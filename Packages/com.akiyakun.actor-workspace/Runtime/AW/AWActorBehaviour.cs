#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public abstract class AWActorBehaviour : IAWActorBehaviour
    {
        public IAWActor Actor { get; private set; }

        public bool ElementActive { get; set; }
        public int ElementPriority { get; set; } = 0;
        // public UpdateFlags UpdateFlags { get; set; } = UpdateFlags.Manual;
        public abstract UpdateFlags UpdateFlags { get; set; }
        public virtual void DoUpdate(float deltaTime) { }
        public virtual void DoLateUpdate(float deltaTime) { }
        public virtual void DoFixedUpdate() { }

        // Factory method
        public static T Create<T>(IAWActor actor)
            where T : AWActorBehaviour, new()
        {
            var behaviour = new T();
            behaviour.Actor = actor;
            return behaviour;
        }

#nullable disable
        protected AWActorBehaviour() { }
#nullable enable

        // public AWActorBehaviour(IAWActor actor)
        // {
        //     Actor = actor;
        //     Debug.Assert(actor != null);
        // }
    }
}
#nullable restore