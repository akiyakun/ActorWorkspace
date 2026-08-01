#nullable enable
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using UnityEngine.Playables;
using UnityEngine.Animations;
using afl;

namespace ActorWorkspace.UnitySpine
{
    public class SpineMecanimAnimation : SpineAnimation
    {
        SkeletonMecanim skeletonMecanim;
        public SkeletonMecanim SkeletonMecanim => skeletonMecanim;

        // Spine.Animation? spineAnimation;
        // public PlayableGraph playableGraph;
        // public AnimationClip? animationClip;
        // public AnimationClipPlayable animationClipPlayable;

        public AnimatorStateOptionInfo stateOptionInfo;

        public SpineMecanimAnimation(AnimatorStateOptionInfo stateOptionInfo, SkeletonMecanim skeletonMecanim)
        {
            this.skeletonMecanim = skeletonMecanim;
            Debug.Assert(skeletonMecanim != null);

            this.stateOptionInfo = stateOptionInfo;
            Name = stateOptionInfo.StateName;
        }

        // public SpineMecanimAnimation(AnimatorStateOptionInfo stateOptionInfo, SkeletonMecanim skeletonMecanim, Spine.Animation? spineAnimation,
        //     PlayableGraph playableGraph, AnimationClip animationClip, AnimationClipPlayable animationClipPlayable)
        // {
        //     this.skeletonMecanim = skeletonMecanim;
        //     Debug.Assert(skeletonMecanim != null);

        //     this.spineAnimation = spineAnimation;
        //     // Debug.Assert(spineAnimation != null);

        //     this.playableGraph = playableGraph;
        //     this.animationClip = animationClip;
        //     this.animationClipPlayable = animationClipPlayable;

        //     this.stateOptionInfo = stateOptionInfo;
        //     Name = stateOptionInfo.StateName;
        // }

        // From IAWAnimation
        // public void Stop()
        // {
        //     // skeletonAnimation.state.SetEmptyAnimation(trackIndex, mixDuration);
        // }
    }
}
#nullable restore