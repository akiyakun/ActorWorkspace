#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public class AWActorBehaviourController : UpdateElementManager<IAWActorBehaviour>, IAWActorBehaviourController
    {
        IAWActor actor;

        #nullable disable
        private AWActorBehaviourController() {}
        #nullable enable

        public AWActorBehaviourController(IAWActor actor)
        {
            this.actor = actor;
            Debug.Assert(actor != null);
        }
    }
}
#nullable restore