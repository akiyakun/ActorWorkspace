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
        public int TrackIndex;

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

    public class SpineSkeletonAnimationController : IAWAnimationController
    {
        public IReadOnlyList<IAWAnimation> AnimationList => animationList;

        public event System.Action<IAWAnimation> OnAnimationComplate;

        SkeletonAnimation skeletonAnimation;
        List<SpineSkeletonAnimation> animationList = new();

        List<SpineTrack> trackList = new List<SpineTrack>();

        public SpineSkeletonAnimationController(SkeletonAnimation skeletonAnimation)
        {
            this.skeletonAnimation = skeletonAnimation;
            Debug.Assert(skeletonAnimation != null);

            // IAWAnimationのリストを作成
            {
                foreach (Spine.Animation animation in skeletonAnimation.Skeleton.Data.Animations)
                {
                    // Debug.Log("Animation name: " + animation.Name);
                    animationList.Add(new SpineSkeletonAnimation(skeletonAnimation, animation));
                }
            }

            // トラックの初期化
            {
                for (int i = 0; i < IAWTrack.MaxTrack; i++)
                {
                    trackList.Add(new SpineTrack(i));
                }
            }
        }


        public void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f)
        {
            var track = trackList[trackIndex];

            // デフォルト(負の値)のときは設定されたデフォルト時間を使う
            if (mixDuration < 0.0f)
            {
                // AnimationStateData stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();
                // mixDuration = stateData.DefaultMix;
                mixDuration = track.MixDuration;
            }

            skeletonAnimation.state.SetEmptyAnimation(trackIndex, mixDuration);
            track.Set(null);
        }
        public IAWTrack SetAnimation(int trackIndex, IAWAnimation animation, bool loop)
        {
            // TrackEntry trackEntry = skeletonAnimation.state.SetAnimation(trackIndex, animation.Name, loop: loop);
            // trackEntry.TimeScale = timeScale;
            var track = trackList[trackIndex];
            track.Set(animation as SpineSkeletonAnimation);
            return track;
        }
        public void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f)
        {
            skeletonAnimation.state.AddAnimation(trackIndex, animation.Name, loop: false, delay: delay);
        }

        public IAWTrack GetTrack(int trackIndex)
        {
            // TrackEntry cu = skeletonAnimation.AnimationState.GetCurrent(trackIndex);
            // if (cu == null) return null;
            // trackList[trackIndex].TrackEntry = cu;
            return trackList[trackIndex];
        }
    }
}
