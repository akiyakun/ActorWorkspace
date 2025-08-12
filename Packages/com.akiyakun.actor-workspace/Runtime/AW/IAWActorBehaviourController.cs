#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // MEMO: MonoBehaviourにしたくなかったやつ
    public interface IAWActorBehaviourController
    {
        public void DoUpdate(float deltaTime);
        public void DoLateUpdate(float deltaTime);
        public void DoFixedUpdate();

        public T Add<T>()
            where T : AWActorBehaviourBase, new();
    }
}
#nullable restore