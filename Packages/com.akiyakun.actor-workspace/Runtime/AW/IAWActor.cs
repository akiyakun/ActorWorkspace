#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public interface IAWActor
    {
        public int ActorCategory { get; }
        public GameObject GameObject { get; }

        public AWActorContextProvider ContextProvider { get; }

        public IAWActorParam ActorParam { get; }
        public IAWActorDisplay ActorDisplay { get; }
        // public IAWAnimation Animation { get; }
        public IAWAnimationController AnimationController { get; }
        public IReadOnlyList<IAWSkin> SkinList { get; }
        public IAWActorBehaviourController ActorBehaviourController { get; }

        // 初期状態に戻す
        public void Restore();

        // public IAWActorParam GetActorParam();
        public void SetSkin(int skinIndex);
    }
}
#nullable restore