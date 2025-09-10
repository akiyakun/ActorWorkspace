using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.Tests
{
    public class FakeAWAnimation : IAWAnimation
    {
        public string Name { get; set; }

        // public IReadOnlyList<AWAnimationData> AnimationList => new List<AWAnimationData>();
        public FakeAWAnimation(string name)
        {
            Name = name;
        }
    }
}
