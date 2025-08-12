#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public interface IAWActorBehaviour : IAsyncInitializable, IUpdateElement
    {
        // public string Name { get; }
    }
}
#nullable restore