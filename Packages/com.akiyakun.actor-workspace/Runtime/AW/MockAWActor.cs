using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    public class MockAWActor : IAWActor
    {
        // MockAWAnimation mockAWAnimation = default;
        MockAWAnimationController mockAWAnimationController = default;

        public GameObject GameObject => null;
        // public IAWAnimation Animation => mockAWAnimation;
        public IAWAnimationController AnimationController => mockAWAnimationController;
        public IReadOnlyList<IAWSkin> SkinList => new List<IAWSkin>();
    }
}
