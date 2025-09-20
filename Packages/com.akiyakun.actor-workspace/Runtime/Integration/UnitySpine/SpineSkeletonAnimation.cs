using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using System.Linq;

namespace ActorWorkspace.UnitySpine
{
    public class SpineSkeletonAnimation : SpineAnimation
    {
        SkeletonAnimation skeletonAnimation;
        public SkeletonAnimation SkeletonAnimation => skeletonAnimation;

        public Spine.Animation SpineAnimation { get; protected set; }

        public SpineSkeletonAnimation(SkeletonAnimation skeletonAnimation, Spine.Animation spineAnimation)
        {
            this.skeletonAnimation = skeletonAnimation;
            Debug.Assert(skeletonAnimation != null);

            SpineAnimation = spineAnimation;
            Debug.Assert(SpineAnimation != null);

            Name = spineAnimation.Name;
        }

        // From IAWAnimation
        // public void Stop()
        // {
        //     // skeletonAnimation.state.SetEmptyAnimation(trackIndex, mixDuration);
        // }
    }
}
