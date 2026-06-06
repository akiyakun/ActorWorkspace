#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;
using ActorWorkspace.ArcaneLedger;

namespace ActorWorkspace
{
    public interface IAWActor : IUpdateElement, System.IDisposable
    {
        public int ActorId { get; }
        public int ActorCategory { get; }
        public GameObject GameObject { get; }

        // public AWActorContextProvider ActorContextProvider { get; }

        public IAWActorParam ActorParam { get; }
        public AWActorDisplay ActorDisplay { get; }
        public IAWAnimationController AnimationController { get; }
        public IReadOnlyList<IAWSkin> SkinList { get; }
        public AWActorBehaviourController ActorBehaviourController { get; }
        public EventBus<string> EventBus { get; }
        public VariableTable Variables { get; }

        // public IALProcessor? ALProcessor { get; }

        // 初期状態に戻す
        public void Restore();

        // public IAWActorParam GetActorParam();
        public void SetSkin(int skinIndex);
    }
}
#nullable restore