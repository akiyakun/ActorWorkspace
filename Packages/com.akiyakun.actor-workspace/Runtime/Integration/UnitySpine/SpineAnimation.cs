#nullable enable
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    // 対になるSpineのクラスはSpine.Animation
    // Spineの SkeletonAnimation と SkeletonMecanim 双方の基底抽象クラス
    public abstract class SpineAnimation : IAWAnimation
    {
        // From IAWAnimation
        public string Name { get; protected set; } = string.Empty;

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
            Debug.Assert(skeletonAnimationInterface != null);

            // this.animationStateComponent = animationStateComponent;
            // Debug.Assert(animationStateComponent != null);

        }

    }
}
#nullable restore