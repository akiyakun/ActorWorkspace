#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.Tests
{
    public class StubAWAnimation : IAWAnimation
    {
        public string Name { get; set; } = null!;

        // public IReadOnlyList<AWAnimationData> AnimationList => new List<AWAnimationData>();
        public StubAWAnimation(string name)
        {
            Name = name;
        }
    }
}
#nullable restore