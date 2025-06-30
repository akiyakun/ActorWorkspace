using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using System.Linq;

namespace ActorWorkspace.UnitySpine
{
    using IntegrationData = SkeletonAnimation;
    public class SpineSkeletonAnimation : IAWAnimation
    {
        public string Name { get; private set; }

        SkeletonAnimation skeletonAnimation;
        Spine.Animation spineAnimation;

        public IReadOnlyList<AWAnimationData> AnimationList => animationDataList;

        public SpineSkeletonAnimation(SkeletonAnimation skeletonAnimation, Spine.Animation spineAnimation)
        {
            this.skeletonAnimation = skeletonAnimation;
            Dbug.Assert(skeletonAnimation != null);

            this.spineAnimation = spineAnimation;
            Dbug.Assert(spineAnimation != null);

            Name = spineAnimation.Name;
        }
    }
}
