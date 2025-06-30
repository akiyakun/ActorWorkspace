using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    public interface IAWActor
    {
        GameObject GameObject { get; }
        // IAWAnimation Animation { get; }
        IAWAnimationController AnimationController { get; }
        IReadOnlyList<IAWSkin> SkinList { get; }
    }

    public interface IAWSkin
    {
        string Name { get; }
    }
    public class MockAWSkin : IAWSkin
    {
        public string Name => "MockSkin";
    }
}
