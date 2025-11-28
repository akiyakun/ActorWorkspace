#nullable enable
using System.Buffers.Text;
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace.Tests
{
    public class StubAWAnimation : IAWAnimation
    {
        public string Name { get; set; } = null!;
        public int NameHash => 0;

        // public IReadOnlyList<AWAnimationData> AnimationList => new List<AWAnimationData>();
        public StubAWAnimation(string name)
        {
            Name = name;
        }
    }
}
#nullable restore