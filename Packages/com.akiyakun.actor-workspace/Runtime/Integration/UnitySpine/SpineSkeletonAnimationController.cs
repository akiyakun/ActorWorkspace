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
    public class SpineSkeletonAnimationController : SpineAnimationControllerBase<SpineSkeletonAnimation>
    {
        public override IAWAnimationParameter AnimationParameter { get; protected set; }

        // FIXME:
        public override bool IsVisibility
        {
            get => skeletonAnimation.GetComponent<Renderer>().enabled;
            set => skeletonAnimation.GetComponent<Renderer>().enabled = value;
        }

        SkeletonAnimation skeletonAnimation;
        IAWAnimationEventDecoder<Spine.Event> animationEventDecoder;

// #nullable disable
//         private SpineSkeletonAnimationController() { }
// #nullable enable

        public SpineSkeletonAnimationController(SkeletonAnimation skeletonAnimation)
            : base(skeletonAnimation)
        {
            this.skeletonAnimation = skeletonAnimation;
            Debug.Assert(skeletonAnimation != null);

            AnimationParameter = new AWAnimationParameter();
            animationEventDecoder = new SpineAnimationEventDecoder();

            // IAWAnimationのリストを作成
            if (skeletonAnimation != null)
            {
                foreach (Spine.Animation animation in skeletonAnimation.Skeleton.Data.Animations)
                {
                    // Debug.Log("Animation name: " + animation.Name);
                    animationHashMap.Add(Utility.StringToHashId(animation.Name), new SpineSkeletonAnimation(skeletonAnimation, animation));
                }

                // コールバック
                skeletonAnimation.AnimationState.Start += OnHandleEntered;
                skeletonAnimation.AnimationState.Complete += OnHandleComplete;
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

        public override void Restore()
        {
        }


        public override void SetEmptyAnimation(AWAnimationOption option = default)
        {
            var track = trackList[option.Track];

            // デフォルト値のときは設定されたデフォルト時間を使う
            if (option.Duration < 0.0f)
            {
                // AnimationStateData stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();
                // mixDuration = stateData.DefaultMix;
                option.Duration = track.MixDuration;
            }

            skeletonAnimation.state.SetEmptyAnimation(option.Track, option.Duration);
            track.Set(null);
        }

        public override IAWTrack? SetAnimation(int hashId, AWAnimationOption option = default)
        {
            var animation = GetAnimationImpl(hashId);
            if (animation == null) throw new System.Exception($"SetAnimation: Not found hashId={hashId}");

            Spine.TrackEntry trackEntry = skeletonAnimation.state.SetAnimation(option.Track, animation.SpineAnimation, loop: option.Loop);

            var track = trackList[option.Track];
            track.Set(animation);
            return track;
        }

#if false
        public override IAWTrack? SetAnimation(int hashId, bool loop = false, int trackNum = 0)
        {
            var animation = GetAnimationImpl(hashId);
            if (animation == null) throw new System.Exception($"SetAnimation: Not found hashId={hashId}");

            Spine.TrackEntry trackEntry = skeletonAnimation.state.SetAnimation(trackNum, animation.SpineAnimation, loop: loop);

            var track = trackList[trackNum];
            track.Set(animation);
            return track;
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
#endif

        public override IAWAnimation? GetCurrentAnimation(int track = 0)
        {
            var currentEntry = skeletonAnimation.AnimationState.GetCurrent(track);
            return currentEntry == null ? null : GetAnimation(currentEntry.Animation.Name);
        }

        public override bool IsPlayingAnimation(int hashId, int track = 0)
        {
            if (GetCurrentAnimation(track) is IAWAnimation anim) return anim.NameHash == hashId;
            return false;
        }

        public override IAWTrack? GetTrack(int trackIndex)
        {
            // TrackEntry cu = skeletonAnimation.AnimationState.GetCurrent(trackIndex);
            // if (cu == null) return null;
            // trackList[trackIndex].TrackEntry = cu;
            return trackList[trackIndex];
        }


        void OnHandleEntered(TrackEntry trackEntry)
        {
            // Debug.Log($"OnHandleEntered: trackIndex={trackEntry.TrackIndex}, animation={trackEntry.Animation?.Name}");
            // if (trackEntry.Loop == true) return;
            var animation = trackList[trackEntry.TrackIndex].Animation;
            if (animation == null) return;
            InvokeAnimationEntered(animation);
        }

        void OnHandleComplete(TrackEntry trackEntry)
        {
            // Debug.Log($"OnHandleComplete: trackIndex={trackEntry.TrackIndex}, animation={trackEntry.Animation?.Name}");
            // if (trackEntry.Loop == true) return;
            var animation = trackList[trackEntry.TrackIndex].Animation;
            if (animation == null) return;
            InvokeAnimationComplate(animation);
        }

        void OnHandleEvent(TrackEntry trackEntry, Spine.Event rawData)
        {
            var animation = trackList[trackEntry.TrackIndex].Animation;
            if (animation == null) return;
            InvokeAnimationEvent(animation, animationEventDecoder.Decode(rawData));
        }
    }
}
#nullable restore