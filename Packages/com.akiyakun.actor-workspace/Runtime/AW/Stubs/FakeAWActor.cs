using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace.Tests
{
    public class FakeAWActor : IAWActor
    {
        public int ActorId { get; set; }
        public int ActorCategory { get; set; }
        public GameObject GameObject => null;

        public AWActorContextProvider ActorContextProvider { get; set; }

        public IAWActorParam ActorParam { get; set; }
        [SerializeField] public AWActorDisplay actorDisplay;
        public AWActorDisplay ActorDisplay => actorDisplay;
        public IAWAnimationController AnimationController { get; set; }
        public IReadOnlyList<IAWSkin> SkinList { get; set; }
        public IAWActorBehaviourController ActorBehaviourController { get; set; }

        #region IUpdateElement
        public bool ElementActive { get; set; }
        public int ElementPriority { get; set; } = 0;
        public UpdateFlags UpdateFlags { get; set; } = UpdateFlags.Update;
        public void DoUpdate(float deltaTime) { }
        public void DoLateUpdate(float deltaTime) { }
        public void DoFixedUpdate() { }
        #endregion

        private FakeAWActor()
        {
        }

        public FakeAWActor(AWActorContextProvider awActorContextProvider, int id, int category)
        {
            ActorId = id;
            ActorCategory = category;

            ActorContextProvider = awActorContextProvider;

            ActorParam = new FakeAWActorParam();
            // ActorDisplay = new FakeAWActorDisplay();
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
