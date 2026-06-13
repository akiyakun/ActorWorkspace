using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // 使い回すときリセットされるもの
    // インターフェースだと都合悪いかも...
    // Variablesがあるのでいらないかもしれない
    public interface IAWActorParam
    {
        // public GameObject GameObject { get; }

        // public Vector3 Position { get; set; }
        public Vector3 ForwardDirection { get; }

        public void Restore();

        // public Vector3 GetPosition();
        // public void SetPosition(Vector3 position);

    }
}
