#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public class AWActorContextProvider : UniversalContextProvider
    {
        public virtual IAWActorManager ActorManager => awActorManager;

#nullable disable
        private AWActorContextProvider() { }
#nullable enable

        IAWActorManager awActorManager;

        public AWActorContextProvider(IAWActorManager awActorManager)
        {
            this.awActorManager = awActorManager;
            Debug.Assert(awActorManager != null);
        }

        public override void Release()
        {
        }
    }
}
#nullable restore