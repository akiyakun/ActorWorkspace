using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.Tests
{
    public class FakeAWActor : IAWActor
    {
        public int ActorCategory { get; set; }
        public GameObject GameObject => null;

        public AWActorContextProvider ContextProvider { get; set; }

        public IAWActorParam ActorParam { get; set; }
        public IAWActorDisplay ActorDisplay { get; set; }
        // public IAWAnimation Animation => mockAWAnimation;
        public IAWAnimationController AnimationController { get; set; }
        public IReadOnlyList<IAWSkin> SkinList { get; set; }
        public IAWActorBehaviourController ActorBehaviourController { get; set; }

        private FakeAWActor()
        {
        }

        public FakeAWActor(AWActorContextProvider awActorContextProvider, int category)
        {
            ActorCategory = category;

            ContextProvider = awActorContextProvider;

            ActorParam = new FakeAWActorParam();
            ActorDisplay = new FakeAWActorDisplay();
            AnimationController = new FakeAWAnimationController();
            SkinList = new List<IAWSkin>();
            ActorBehaviourController = new AWActorBehaviourController(this);
        }

        public void Restore()
        {
            ActorParam.Restore();
        }

        public IAWActorParam GetActorParam()
        {
            return ActorParam;
        }

        public void SetSkin(int skinIndex)
        {
        }
    }
}
