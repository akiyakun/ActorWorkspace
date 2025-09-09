#nullable enable
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using afl;

namespace ActorWorkspace.UnitySpine
{
    // SkeletonMecanim と対になるコントローラークラス
    public class SpineMecanimAnimationController : SpineAnimationController
    {
        SkeletonMecanim skeletonMecanim;

        public SpineMecanimAnimationController(SkeletonMecanim skeletonMecanim, IAWEventDecoder eventDecoder)
            : base(skeletonMecanim, eventDecoder)
        {
            this.skeletonMecanim = skeletonMecanim;
            // Debug.Assert(skeletonMecanim != null);


#if UNITY_EDITOR
            // チェック用にAnimatorControllerの全ステートを取得
            Dictionary<string, UnityEditor.Animations.AnimatorState> states = new();
            {
                Animator animator = skeletonMecanim.GetComponent<Animator>();
                var controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                foreach (var layer in controller!.layers)
                {
                    Debug.Log($"Layer: {layer.name}");
                    foreach (var state in layer.stateMachine.states)
                    {
                        Debug.Log($"State: {state.state.name}");
                        states.Add(state.state.name, state.state);

                        // Mecanimのステート名をIAWAnimationとして登録
                        animations.Add(state.state.name, new SpineMecanimAnimation(state.state.name, skeletonMecanim, null));
                    }
                }
            }
#endif

            // IAWAnimationのリストを作成
            // if (skeletonMecanim != null)
            {
                foreach (Spine.Animation animation in skeletonMecanim.Skeleton.Data.Animations)
                {
                    // このanimationはSpineの元データでありMecanimのステートではない
                    // Debug.Log("Animation name: " + animation.Name);
                    // animations.Add(animation.Name, new SpineMecanimAnimation(skeletonMecanim, animation));

// #if UNITY_EDITOR
//                     // AnimatorControllerのステート存在チェック
//                     if (states.Get(animation.Name) == null)
//                     {
//                         Debug.LogWarning($"Animator: AnimationState not found. name={animation.Name}");
//                     }
// #endif
                }

                // コールバック
                // FIXME: 終了処理
                // skeletonMecanim.AnimationState.Event += OnHandleEvent;
                var detector = skeletonMecanim.gameObject.GetComponent<SpineMecanimAnimationEventDetector>();
                if (detector != null)
                {
                    detector.Setup(this);
                }
            }
        }


        public override void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f)
        {
            // var track = trackList[trackIndex];

            // // デフォルト(負の値)のときは設定されたデフォルト時間を使う
            // if (mixDuration < 0.0f)
            // {
            //     // AnimationStateData stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();
            //     // mixDuration = stateData.DefaultMix;
            //     mixDuration = track.MixDuration;
            // }

            // skeletonAnimation.state.SetEmptyAnimation(trackIndex, mixDuration);
            // track.Set(null);
        }
        public override IAWTrack? SetAnimation(int trackIndex, IAWAnimation animation, bool loop)
        {
            if (animation == null)
            {
                Debug.Assert(false, $"SetAnimation: trackIndex={trackIndex}, animation={animation?.Name}, loop={loop}");
                return null;
            }

            Animator animator = skeletonMecanim.GetComponent<Animator>();
            Debug.Log(animation.Name);
            animator.Play(stateName: animation.Name, layer: trackIndex);

            var track = trackList[trackIndex];
            // track.Set(animation as SpineSkeletonAnimation);
            return track;

            // MEMO:
            // Animator.Play()が成功したかどうかを取得する手段が無い
            // Play()後に現在のステートをチェックすることも可能だが、直後にステートが変わるとは限らない
            //
            // var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex: 0);
            // if (!stateInfo.IsName(animation.Name))
            // {
            //     Debug.LogWarning($"SetAnimation: trackIndex={trackIndex}, animation={animation?.Name}, loop={loop}");
            // }

            // if (animation != null)
            // {
            //     TrackEntry trackEntry = skeletonAnimation.state.SetAnimation(trackIndex, animation.Name, loop: loop);
            //     // trackEntry.TimeScale = timeScale;
            // }
            // else
            // {
            //     Debug.Assert(false, $"SetAnimation: trackIndex={trackIndex}, animation={animation?.Name}, loop={loop}");
            //     return null;
            // }

            // var track = trackList[trackIndex];
            // track.Set(animation as SpineSkeletonAnimation);
            // return track;
        }
        public override void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f)
        {
            // skeletonAnimation.state.AddAnimation(trackIndex, animation.Name, loop: false, delay: delay);
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
            InvokeAnimationEvent(animation, eventDecoder.Decode(spineEvent));
        }
        public void OnSpineEvent(string eventName, float eventTime, int intValue, float floatValue, string stringValue)
        {
            // FIXME:
            var animation = trackList[0].Animation;
            AWEventData eventData = new AWEventData
            {
                Name = eventName,
                Int = intValue,
                Float = floatValue,
                String = stringValue,
            };
            // InvokeAnimationEvent(animation, eventDecoder.Decode(eventName));
            InvokeAnimationEvent(animation, eventData);
        }
    }
}
#nullable restore