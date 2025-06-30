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
        public float TimeScale
        {
            get => TrackEntry.TimeScale;
            set => TrackEntry.TimeScale = value;
        }

        public float MixDuration
        {
            get => TrackEntry.DefaultMix;
            set => TrackEntry.DefaultMix = value;
        }

        public IAWAnimation Animation => TrackEntry?.Animation;
        public TrackEntry TrackEntry { get; set; }

        public void Set(TrackEntry trackEntry)
        {
            if (TrackEntry != null)
            {
                TrackEntry = trackEntry;
            }
            else
            {
                TrackEntry = null;
            }
        }

    }

    public class SpineSkeletonAnimationController : IAWAnimationController
    {
        public IReadOnlyList<IAWAnimation> AnimationList => animationList;

        public event System.Action<IAWAnimation> OnAnimationComplate;

        SkeletonAnimation skeletonAnimation;
        List<IAWAnimation> animationList = new List<IAWAnimation>();

        List<SpineTrack> trackList = new List<SpineTrack>();

        public SpineSkeletonAnimationController(SkeletonAnimation skeletonAnimation)
        {
            this.skeletonAnimation = skeletonAnimation;
            Dbug.Assert(skeletonAnimation != null);

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
                    trackList.Add(new SpineTrack());
                }
            }
        }


        public void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f)
        {
            // デフォルト(負の値)のときは設定されたデフォルト時間を使う
            if (mixDuration < 0.0f)
            {
                AnimationStateData stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();
                mixDuration = stateData.DefaultMix;
            }

            skeletonAnimation.state.SetEmptyAnimation(trackIndex, mixDuration);
            var track = trackList[trackIndex];
            track.Set(null);
        }
        public IAWTrack SetAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f)
        {
            TrackEntry trackEntry = skeletonAnimation.state.SetAnimation(trackIndex, animation, delay: delay, loop: loop);
            // trackEntry.TimeScale = timeScale;
            var track = trackList[trackIndex];
            track.Set(trackEntry);
            return track;
        }
        public void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f)
        {
            skeletonAnimation.state.AddAnimation(trackIndex, animation, loop: false, delay: delay);
        }

        public AWTrack GetTrack(int trackIndex)
        {
            TrackEntry cu = skeletonAnimation.AnimationState.GetCurrent(trackIndex);
            if (cu == null) return null;
            return trackList[trackIndex];
        }
    }
}
