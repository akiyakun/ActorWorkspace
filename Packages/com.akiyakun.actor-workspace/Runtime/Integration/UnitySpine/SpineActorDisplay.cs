using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using System.Linq;

namespace ActorWorkspace.UnitySpine
{
    public class SpineActorDisplay : IAWActorDisplay
    {
        ISkeletonAnimation skeletonAnimation;
        public ISkeletonAnimation SkeletonAnimation => skeletonAnimation;

        public SpineActorDisplay(ISkeletonAnimation skeletonAnimation)
        {
            this.skeletonAnimation = skeletonAnimation;
            Debug.Assert(skeletonAnimation != null);
        }
    }
}
