#nullable enable
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using afl;

namespace ActorWorkspace.UnitySpine
{
    // 対になるSpineのクラスはSpine.Animation
    // Spineの SkeletonAnimation と SkeletonMecanim 双方の基底抽象クラス
    public abstract class SpineAnimation : IAWAnimation
    {
        string name = "";

        // From IAWAnimation
        public string Name
        {
            get => name;
            protected set
            {
                name = value;
                NameHash = Utility.StringToHashId(name);
            }
        }

        // From IAWAnimation
        public int NameHash { get; private set; }

        // SkeletonAnimation と SkeletonMecanim 双方が継承しているインターフェース
        ISkeletonAnimation skeletonAnimationInterface;

        // IAnimationStateComponent animationStateComponent;
        // public Spine.AnimationState State => animationStateComponent.AnimationState;

#nullable disable
        protected SpineAnimation() { }
#nullable enable

        public SpineAnimation(ISkeletonAnimation skeletonAnimationInterface)
        {
            this.skeletonAnimationInterface = skeletonAnimationInterface;
            if (skeletonAnimationInterface == null) throw new System.ArgumentNullException(nameof(skeletonAnimationInterface));

            // this.animationStateComponent = animationStateComponent;
            // Debug.Assert(animationStateComponent != null);

        }

    }
}
#nullable restore