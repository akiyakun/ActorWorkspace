#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // MEMO:
    // MonoBehaviourぽく使いたいのでメソッド名も似せてあります
    // ベースクラスのメソッド呼び出しを忘れてしまうことがあるので、
    // Do系とOn系で分けています、ベースクラスなどはDo系をoverrideするとよいでしょう。
    public abstract class AWActorBehaviour<TActor> : IAWActorBehaviour
        where TActor : class, IAWActor
    {
        public bool IsActive { get; protected set; } = true;

        IAWActor IAWActorBehaviour.Actor => Actor as IAWActor;
        public TActor Actor { get; private set; } = null!;

        public EventBus<string> EventBus => Actor.EventBus;
        public VariableTable Variables => Actor.Variables;
        public EventBag EventBag { get; private set; } = new();

        #region IUpdateElement
        public bool ElementActive { get; set; }
        public int ElementPriority { get; protected set; } = 0;
        public virtual UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Manual;
        // public abstract UpdateFlags UpdateFlags { get; set; }
        // public virtual void DoLateUpdate(float deltaTime) { }
        // public virtual void DoFixedUpdate(float deltaTime) { }
        #endregion

        bool isFirstUpdate = true;

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
            Debug.Assert(Actor != null);
        }

        public virtual void Terminate()
        {
            EventBag?.Dispose();
        }

        // From IActorBehaviour
        public virtual void Restore()
        {
            OnRestore();
            isFirstUpdate = true;
        }

        // From IActorBehaviour
        public virtual void Awake() => OnAwake();

        // From IActorBehaviour
        public virtual void Start() => OnStart();

        // From IUpdateElement
        public virtual void DoUpdate(float deltaTime)
        {
            if (isFirstUpdate == true)
            {
                isFirstUpdate = false;
                Start();
            }
            OnUpdate(deltaTime);
        }

        // From IUpdateElement
        public virtual void DoLateUpdate(float deltaTime)
        {
            if (isFirstUpdate == true)
            {
                isFirstUpdate = false;
                Start();
            }
            OnLateUpdate(deltaTime);
        }

        // From IUpdateElement
        public virtual void DoFixedUpdate(float deltaTime)
        {
            if (isFirstUpdate == true)
            {
                isFirstUpdate = false;
                Start();
            }
            OnFixedUpdate(deltaTime);
        }

        // From IActorBehaviour
        public virtual void Destroy()
        {
            OnDestroy();
            EventBag?.Dispose();
        }

        public abstract void OnRestore();
        public virtual void OnAwake() {}
        public virtual void OnStart() {}
        public virtual void OnUpdate(float deltaTime) {}
        public virtual void OnLateUpdate(float deltaTime) {}
        public virtual void OnFixedUpdate(float deltaTime) {}
        public virtual void OnDestroy() {}

    }
}
#nullable restore