using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using System.Linq;

namespace ActorWorkspace.UnitySpine
{
    public class SpineMecanimAnimation : SpineAnimation
    {
        SkeletonMecanim skeletonMecanim;
        public SkeletonMecanim SkeletonMecanim => skeletonMecanim;

        Spine.Animation spineAnimation;

        public SpineMecanimAnimation(SkeletonMecanim skeletonMecanim, Spine.Animation spineAnimation)
        {
            this.skeletonMecanim = skeletonMecanim;
            Debug.Assert(skeletonMecanim != null);

            this.spineAnimation = spineAnimation;
            Debug.Assert(spineAnimation != null);

            Name = spineAnimation.Name;
        }

        // From IAWAnimation
        // public void Stop()
        // {
        //     // skeletonAnimation.state.SetEmptyAnimation(trackIndex, mixDuration);
        // }
    }
}
