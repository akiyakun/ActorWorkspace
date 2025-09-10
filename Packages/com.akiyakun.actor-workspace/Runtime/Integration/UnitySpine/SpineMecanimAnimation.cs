#nullable enable
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace ActorWorkspace.UnitySpine
{
    public class SpineMecanimAnimation : SpineAnimation
    {
        SkeletonMecanim skeletonMecanim;
        public SkeletonMecanim SkeletonMecanim => skeletonMecanim;

        Spine.Animation? spineAnimation;
        public PlayableGraph playableGraph;
        public AnimationClip? animationClip;
        public AnimationClipPlayable animationClipPlayable;

        public SpineMecanimAnimation(string stateName, SkeletonMecanim skeletonMecanim, Spine.Animation? spineAnimation,
            PlayableGraph playableGraph, AnimationClip animationClip, AnimationClipPlayable animationClipPlayable)
        {
            this.skeletonMecanim = skeletonMecanim;
            Debug.Assert(skeletonMecanim != null);

            this.spineAnimation = spineAnimation;
            // Debug.Assert(spineAnimation != null);

            this.playableGraph = playableGraph;
            this.animationClip = animationClip;
            this.animationClipPlayable = animationClipPlayable;

            // Name = spineAnimation.Name;
            Name = stateName;
        }

        // From IAWAnimation
        // public void Stop()
        // {
        //     // skeletonAnimation.state.SetEmptyAnimation(trackIndex, mixDuration);
        // }
    }
}
#nullable restore