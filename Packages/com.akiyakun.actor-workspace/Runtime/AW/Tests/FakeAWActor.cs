using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.Tests
{
    public class FakeAWActor : IAWActor
    {
        public int ActorCategory { get; set; }
        public GameObject GameObject => null;
        public IAWActorParam IActorParam { get; set; }

        public IAWActorDisplay ActorDisplay { get; set; }
        // public IAWAnimation Animation => mockAWAnimation;
        public IAWAnimationController AnimationController { get; set; }
        public IReadOnlyList<IAWSkin> SkinList { get; set; }

        private FakeAWActor()
        {
        }

        public FakeAWActor(int category)
        {
            ActorCategory = category;
            IActorParam = new FakeAWActorParam();

            ActorDisplay = new FakeAWActorDisplay();
            AnimationController = new FakeAWAnimationController();
            SkinList = new List<IAWSkin>();
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
