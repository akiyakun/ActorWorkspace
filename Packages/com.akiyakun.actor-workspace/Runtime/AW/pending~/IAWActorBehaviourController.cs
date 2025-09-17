#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // MEMO: MonoBehaviourにしたくなかったやつ
    public interface IAWActorBehaviourController : System.IDisposable
    {
        public event System.Action<IAWActorBehaviour> OnBehaviourAdded;
        public event System.Action<IAWActorBehaviour> OnBehaviourRemoved;

        public void Restore();

        public void DoUpdate(float deltaTime);
        public void DoLateUpdate(float deltaTime);
        public void DoFixedUpdate();

        public T? Add<T>()
            where T : IAWActorBehaviour, new();

        public bool Remove<T>()
            where T : IAWActorBehaviour;

        public T? Get<T>()
            where T : IAWActorBehaviour;
    }
}
#nullable restore