#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Spine;
using Spine.Unity;
using afl;

namespace ActorWorkspace.UnitySpine
{
    // SkeletonAnimation と対になるコントローラークラス
    public class SpineSkeletonAnimationController : SpineAnimationController
    {
        SkeletonAnimation skeletonAnimation;

// #nullable disable
//         private SpineSkeletonAnimationController() { }
// #nullable enable

        public SpineSkeletonAnimationController(SkeletonAnimation skeletonAnimation, IAWEventDecoder eventDecoder)
            : base(skeletonAnimation, eventDecoder)
        {
            this.skeletonAnimation = skeletonAnimation;
            Debug.Assert(skeletonAnimation != null);

            // IAWAnimationのリストを作成
            if (skeletonAnimation != null)
            {
                foreach (Spine.Animation animation in skeletonAnimation.Skeleton.Data.Animations)
                {
                    // Debug.Log("Animation name: " + animation.Name);
                    animations.Add(animation.Name, new SpineSkeletonAnimation(skeletonAnimation, animation));
                }

                // コールバック
                // FIXME: 終了処理
                skeletonAnimation.AnimationState.Event += OnHandleEvent;
            }
        }

        public override async UniTask<int> InitializeAsync(CancellationToken cancellationToken)
        {
            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        public override void Dispose()
        {
            if (skeletonAnimation != null)
            {
                skeletonAnimation.AnimationState.Event -= OnHandleEvent;
            }
        }


        public override void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f)
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
        public override IAWTrack? SetAnimation(int trackIndex, IAWAnimation animation, bool loop)
        {
            if (animation != null)
            {
                TrackEntry trackEntry = skeletonAnimation.state.SetAnimation(trackIndex, animation.Name, loop: loop);
                // trackEntry.TimeScale = timeScale;
            }
            else
            {
                Debug.Assert(false, $"SetAnimation: trackIndex={trackIndex}, animation={animation?.Name}, loop={loop}");
                return null;
            }

            var track = trackList[trackIndex];
            track.Set(animation as SpineSkeletonAnimation);
            return track;
        }
        public override void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f)
        {
            skeletonAnimation.state.AddAnimation(trackIndex, animation.Name, loop: false, delay: delay);
        }

        public override IAWTrack? GetTrack(int trackIndex)
        {
            // TrackEntry cu = skeletonAnimation.AnimationState.GetCurrent(trackIndex);
            // if (cu == null) return null;
            // trackList[trackIndex].TrackEntry = cu;
            return trackList[trackIndex];
        }


        void OnHandleEvent(TrackEntry trackEntry, Spine.Event spineEvent)
        {
            var animation = trackList[trackEntry.TrackIndex].Animation;
            if (animation == null) return;
            InvokeAnimationEvent(animation, eventDecoder.Decode(spineEvent));
        }
    }
}
#nullable restore