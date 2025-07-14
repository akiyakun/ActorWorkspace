using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // 使い回すときリセットされるもの
    public interface IAWActorParam
    {
        // public GameObject GameObject { get; }

        public void Restore();
    }
}
