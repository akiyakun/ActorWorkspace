#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // MEMO:
    // MonoBehaviourぽく使いたいのでメソッド名も似せてあります
    public abstract class AWActorBehaviour<TActor> : IAWActorBehaviour
        where TActor : class, IAWActor
    {
        IAWActor IAWActorBehaviour.Actor => Actor as IAWActor;
        public TActor Actor { get; private set; } = null!;

        public EventBus<string> EventBus => Actor.EventBus;
        public VariableTable Variables => Actor.Variables;
        public EventBag EventBag { get; private set; } = new();

        #region IUpdateElement
        public bool ElementActive { get; set; }
        public int ElementPriority { get; set; } = 0;
        public virtual UpdateFlags UpdateFlags { get; set; } = UpdateFlags.Manual;
        // public abstract UpdateFlags UpdateFlags { get; set; }
        public virtual void DoUpdate(float deltaTime) { }
        public virtual void DoLateUpdate(float deltaTime) { }
        public virtual void DoFixedUpdate() { }
        #endregion

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
            Actor = (TActor)actor;
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