#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.Tests
{
    public class StubAWActorParam : IAWActorParam
    {
        public int Cluster { get; set; }

        // public Vector3 Position;
        public Vector3 ForwardDirection { get; set; }

        public void Restore()
        {
        }

        // public Vector3 GetPosition() => Position;
        // public void SetPosition(Vector3 position) => Position = position;

    }
}
#nullable restore