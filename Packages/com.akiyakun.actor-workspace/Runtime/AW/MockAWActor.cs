using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    public class MockAWActor : IAWActor
    {
        public int ActorCategory { get; set; }
        public GameObject GameObject => null;
        public IAWActorParam IActorParam { get; set; }

        // public IAWAnimation Animation => mockAWAnimation;
        public IAWAnimationController AnimationController { get; set; } = new MockAWAnimationController();
        public IReadOnlyList<IAWSkin> SkinList => new List<IAWSkin>();

        public MockAWActor(int category)
        {
            ActorCategory = category;
            IActorParam = new MockAWActorParam();
        }

        public void Restore()
        {
            IActorParam.Restore();
        }

        public IAWActorParam GetActorParam()
        {
            return IActorParam;
        }

        public void SetSkin(int skinIndex)
        {
        }
    }
}
