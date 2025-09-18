#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // MEMO:
    // MonoBehaviourぽく使いたいのでメソッド名も似せてあります
    public abstract class AWActorBehaviour<TActor> : IAWActorBehaviour
        where TActor : IAWActor
    {
        public TActor Actor { get; private set; }

        public bool ElementActive { get; set; }
        public int ElementPriority { get; set; } = 0;
        // public UpdateFlags UpdateFlags { get; set; } = UpdateFlags.Manual;
        public abstract UpdateFlags UpdateFlags { get; set; }
        public virtual void DoUpdate(float deltaTime) { }
        public virtual void DoLateUpdate(float deltaTime) { }
        public virtual void DoFixedUpdate() { }

        // Factory method
        // public static T Create<T>(IAWActor actor)
        //     where T : AWActorBehaviour<TActor>, new()
        // {
        //     var behaviour = new T();
        //     behaviour.Actor = actor;
        //     return behaviour;
        // }

#nullable disable
        protected AWActorBehaviour() { }
#nullable enable

        // public AWActorBehaviour(IAWActor actor)
        // {
        //     Actor = actor;
        //     Debug.Assert(actor != null);
        // }

        public virtual void Initialize(IAWActor actor)
        {
            Debug.Assert(Actor == null);

            Actor = (TActor)actor!;
            Debug.Assert(Actor != null);
        }

        public virtual void Terminate()
        {
        }

        public abstract void Restore();

        public virtual void DoAwake()
        {
        }

        public virtual void DoDestroy()
        {
        }

    }
}
#nullable restore