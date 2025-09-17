using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // 使い回すときリセットされるもの
    // インターフェースだと都合悪いかも...
    public interface IAWActorParam
    {
        // public GameObject GameObject { get; }

        // public Vector3 Position { get; set; }

        public void Restore();
    }
}
