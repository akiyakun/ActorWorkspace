using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using System.Linq;

namespace ActorWorkspace.UnitySpine
{
    public class SpineSkeletonAnimation : IAWAnimation
    {
        public string Name { get; private set; }

        SkeletonAnimation skeletonAnimation;
        public SkeletonAnimation SkeletonAnimation => skeletonAnimation;

        Spine.Animation spineAnimation;

        public SpineSkeletonAnimation(SkeletonAnimation skeletonAnimation, Spine.Animation spineAnimation)
        {
            this.skeletonAnimation = skeletonAnimation;
            Debug.Assert(skeletonAnimation != null);

            this.spineAnimation = spineAnimation;
            Debug.Assert(spineAnimation != null);

            Name = spineAnimation.Name;
        }
    }
}
