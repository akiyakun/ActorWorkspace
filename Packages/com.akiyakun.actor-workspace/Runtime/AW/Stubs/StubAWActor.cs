#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;
using ActorWorkspace.ArcaneLedger;

namespace ActorWorkspace.Tests
{
    public class StubAWActor : IAWActor
    {
        public int ActorId { get; set; }
        public int ActorCategory { get; set; }
        public GameObject GameObject => null!;

        // public AWActorContextProvider ActorContextProvider { get; set; } = null!;

        public IAWActorParam ActorParam { get; set; } = null!;
        [SerializeField] public AWActorDisplay actorDisplay = null!;
        public AWActorDisplay ActorDisplay => actorDisplay;
        public IAWAnimationController AnimationController { get; set; } = null!;
        public IReadOnlyList<IAWSkin> SkinList { get; set; } = null!;
        public AWActorBehaviourController ActorBehaviourController { get; set; } = null!;
        public EventBus<string> EventBus { get; set; } = new();
        public VariableTable Variables { get; set; } = new();

        public IALProcessor? ALProcessor { get; set; } = null;

        #region IUpdateElement
        public bool ElementActive { get; set; }
        public int ElementPriority { get; set; } = 0;
        public UpdateFlags UpdateFlags { get; set; } = UpdateFlags.Update;
        public void DoUpdate(float deltaTime) { }
        public void DoLateUpdate(float deltaTime) { }
        public void DoFixedUpdate(float deltaTime) { }
        #endregion

        private StubAWActor()
        {
        }

        public StubAWActor(int id, int category)
        {
            ActorId = id;
            ActorCategory = category;

            // ActorContextProvider = awActorContextProvider;

            ActorParam = new StubAWActorParam();
            // ActorDisplay = new FakeAWActorDisplay();
            AnimationController = new StubAWAnimationController();
            SkinList = new List<IAWSkin>();
            ActorBehaviourController = new AWActorBehaviourController(this);
        }

        public void Dispose()
        {
            ActorBehaviourController?.Dispose();
            ActorBehaviourController = null!;

            AnimationController?.Dispose();
            AnimationController = null!;
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
#nullable restore