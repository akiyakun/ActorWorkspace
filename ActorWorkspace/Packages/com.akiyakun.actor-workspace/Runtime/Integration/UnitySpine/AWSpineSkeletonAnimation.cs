using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using System.Linq;

namespace ActorWorkspace.UnitySpine
{
    public class AWSpineSkeletonAnimation : IAWAnimation
    {
        public SkeletonAnimation skeletonAnimation;

        public IEnumerable<AWAnimationData> AnimationList
        {
            get
            {
                return skeletonAnimation.Skeleton.Data.Animations.Cast<AWAnimationData>();
            }
        }

        void InnerSetup()
        {
            // SkeletonData skeletonData = skeletonAnimation.Skeleton.Data;
            // foreach (Spine.Animation animation in skeletonData.Animations)
            // {
            //     Debug.Log("Animation name: " + animation.Name);
            // }
        }
    }
}
