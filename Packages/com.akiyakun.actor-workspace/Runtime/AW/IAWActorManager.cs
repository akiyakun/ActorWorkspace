#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public interface IAWActorManager
    {
        public void DoUpdate(float deltaTime);
		public void DoLateUpdate(float deltaTime);
        public void DoFixedUpdate();

        public bool Add(IAWActor actor);
        public bool Remove(IAWActor actor);
    }
}
#nullable restore