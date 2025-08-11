using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.Tests
{
    public class FakeAWAnimation : IAWAnimation
    {
        public string Name => nameof(FakeAWAnimation);

        // public IReadOnlyList<AWAnimationData> AnimationList => new List<AWAnimationData>();

    }
}
