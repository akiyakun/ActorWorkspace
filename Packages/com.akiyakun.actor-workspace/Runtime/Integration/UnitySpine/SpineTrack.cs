using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    // SpineのTrackは同じインデックスのインスタンスがずっと保持されるわけではなさそう
    // だけど使用するときはそのトラックのスピード等は保存されていて欲しい
    public class SpineTrack : IAWTrack
    {
        public int TrackIndex { get; protected set; }

        float timeScale = 1.0f;
        public override float TimeScale
        {
            get => timeScale;
            set => SetTimeScale(value);
        }

        public override float MixDuration { get; set; }
        // {
        //     get => TrackEntry.DefaultMix;
        //     set => TrackEntry.DefaultMix = value;
        // }

        public override IAWAnimation Animation { get; protected set; }
        public TrackEntry TrackEntry { get; set; }

        SpineSkeletonAnimation spineSkeletonAnimation;

        private SpineTrack() { }
        public SpineTrack(int trackIndex)
        {
            TrackIndex = trackIndex;
        }

        public void Set(SpineSkeletonAnimation spineSkeletonAnimation)
        {
            this.spineSkeletonAnimation = spineSkeletonAnimation;
            Animation = spineSkeletonAnimation as IAWAnimation;
        }

        public void SetTimeScale(float value)
        {
            timeScale = value;

            // SkeletonAnimationが存在するならそのトラックのTimeScaleにも反映させる
            if (spineSkeletonAnimation == null) return;
            TrackEntry trackEntry = spineSkeletonAnimation.SkeletonAnimation.AnimationState.GetCurrent(TrackIndex);
            if (trackEntry != null)
            {
                trackEntry.TimeScale = timeScale;
            }
        }

    }
}
