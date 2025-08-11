using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    // MEMO: SkeletonMecanimのインターフェイスが違いすぎて大変なので実装保留
    // public class SpineSkeletonMecanimController : IAWAnimationController
    // {
    //     public IReadOnlyList<IAWAnimation> AnimationList => animationList;

    //     public event System.Action<IAWAnimation> OnAnimationComplate;
    //     public event System.Action<IAWAnimation, AWEventData> OnAnimationEvent;

    //     SkeletonAnimation skeletonAnimation;
    //     IAWEventDecoder eventDecoder;

    //     List<SpineSkeletonAnimation> animationList = new();

    //     List<SpineTrack> trackList = new(IAWTrack.MaxTrack);


    //     public SpineSkeletonAnimationController(SkeletonMecanim skeletonMecanim, IAWEventDecoder eventDecoder)
    //     {
    //         this.skeletonAnimation = skeletonAnimation;
    //         Debug.Assert(skeletonAnimation != null);

    //         this.eventDecoder = eventDecoder;
    //         Debug.Assert(eventDecoder != null);

    //         // IAWAnimationのリストを作成
    //         {
    //             foreach (Spine.Animation animation in skeletonAnimation.Skeleton.Data.Animations)
    //             {
    //                 // Debug.Log("Animation name: " + animation.Name);
    //                 animationList.Add(new SpineSkeletonAnimation(skeletonAnimation, animation));
    //             }
    //         }

    //         // トラックの初期化
    //         {
    //             for (int i = 0; i < IAWTrack.MaxTrack; i++)
    //             {
    //                 trackList.Add(new SpineTrack(i));
    //             }
    //         }

    //         // コールバック
    //         // FIXME: 終了処理
    //         skeletonAnimation.AnimationState.Event += OnHandleEvent;
    //     }


    //     public void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f)
    //     {
    //         var track = trackList[trackIndex];

    //         // デフォルト(負の値)のときは設定されたデフォルト時間を使う
    //         if (mixDuration < 0.0f)
    //         {
    //             // AnimationStateData stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();
    //             // mixDuration = stateData.DefaultMix;
    //             mixDuration = track.MixDuration;
    //         }

    //         skeletonAnimation.state.SetEmptyAnimation(trackIndex, mixDuration);
    //         track.Set(null);
    //     }
    //     public IAWTrack SetAnimation(int trackIndex, IAWAnimation animation, bool loop)
    //     {
    //         if (animation != null)
    //         {
    //             TrackEntry trackEntry = skeletonAnimation.state.SetAnimation(trackIndex, animation.Name, loop: loop);
    //             // trackEntry.TimeScale = timeScale;
    //         }

    //         var track = trackList[trackIndex];
    //         track.Set(animation as SpineSkeletonAnimation);
    //         return track;
    //     }
    //     public void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f)
    //     {
    //         skeletonAnimation.state.AddAnimation(trackIndex, animation.Name, loop: false, delay: delay);
    //     }

    //     public IAWTrack GetTrack(int trackIndex)
    //     {
    //         // TrackEntry cu = skeletonAnimation.AnimationState.GetCurrent(trackIndex);
    //         // if (cu == null) return null;
    //         // trackList[trackIndex].TrackEntry = cu;
    //         return trackList[trackIndex];
    //     }


    //     void OnHandleEvent(TrackEntry trackEntry, Spine.Event spineEvent)
    //     {
    //         var animation = trackList[trackEntry.TrackIndex].Animation;
    //         OnAnimationEvent?.Invoke(animation, eventDecoder.Decode(spineEvent));
    //     }
    // }
}
