#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // MEMO: MonoBehaviourにしたくなかったやつ
    public interface IAWActorBehaviourController : System.IDisposable
    {
        public event System.Action<AWActorBehaviour> OnBehaviourAdded;
        public event System.Action<AWActorBehaviour> OnBehaviourRemoved;

        public void Restore();

        public void DoUpdate(float deltaTime);
        public void DoLateUpdate(float deltaTime);
        public void DoFixedUpdate();

        public T? Add<T>()
            where T : AWActorBehaviour, new();

        public bool Remove<T>()
            where T : AWActorBehaviour;

        public T? Get<T>()
            where T : AWActorBehaviour;
    }
}
#nullable restore