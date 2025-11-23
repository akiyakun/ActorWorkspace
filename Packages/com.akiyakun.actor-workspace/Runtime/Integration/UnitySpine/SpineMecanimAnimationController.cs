#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Spine.Unity;
using afl;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace ActorWorkspace.UnitySpine
{
    // SkeletonMecanim と対になるコントローラークラス
    //
    // MEMO:
    // MecanimのループはAnimationClipのLoppTimeを設定するのが前提で、基本的に実行中にループ設定することができない仕様。
    // 今回はループあり・なしで別々のステートを用意し、名前で区別する運用とした。
    // ループなし:run
    // ループあり:run_loop
    // 対象のステート名が無い場合エラーになります
    public class SpineMecanimAnimationController : SpineAnimationControllerBase<SpineMecanimAnimation>
    {
        public override IAWAnimationParameter AnimationParameter { get; protected set; }

        // FIXME:
        public override bool IsVisibility
        {
            get => skeletonMecanim.GetComponent<Renderer>().enabled;
            set => skeletonMecanim.GetComponent<Renderer>().enabled = value;
        }

        SkeletonMecanim skeletonMecanim;
        Animator animator;

#if UNITY_EDITOR
        Dictionary<string, UnityEditor.Animations.AnimatorState> states = new();
#endif

        public SpineMecanimAnimationController(SkeletonMecanim skeletonMecanim, IAWEventDecoder eventDecoder)
            : base(skeletonMecanim, eventDecoder)
        {
            this.skeletonMecanim = skeletonMecanim;
            // Debug.Assert(skeletonMecanim != null);

            animator = skeletonMecanim.GetComponent<Animator>();
            if (animator == null) throw new System.Exception("Animator component not found");

            AnimationParameter = new MecanimAnimationParameter(animator);

#if false
// #if UNITY_EDITOR
            // チェック用にAnimatorControllerの全ステートを取得
            {
                var controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                foreach (var layer in controller!.layers)
                {
                    Debug.Log($"Layer: {layer.name}");
                    foreach (var state in layer.stateMachine.states)
                    {
                        Debug.Log($"State: {state.state.name}");

                        if (state.state.motion is AnimationClip animationClip)
                        {
                            states.Add(state.state.name, state.state);

                            var playableGraph = PlayableGraph.Create();
                            var output = AnimationPlayableOutput.Create(playableGraph, "SpineOutput", animator);

                            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);
                            output.SetSourcePlayable(clipPlayable);

                            // Mecanimのステート名をIAWAnimationとして登録
                            animationHashMap.Add(Utility.StringToHashId(state.state.name),
                                new SpineMecanimAnimation(state.state.name, skeletonMecanim, null, playableGraph, animationClip, clipPlayable));

                            playableGraph.Play();
                        }
                        // else if (motion is BlendTree blendTree)
                        // {
                        //     GetClipsFromBlendTree(blendTree);
                        // }
                    }
                }
            }
#endif

            // IAWAnimationのリストを作成
            // if (skeletonMecanim != null)
            {
                var animatorStateEvent = AnimatorStateEvent.Get(animator, 0);
                if (animatorStateEvent == null) throw new System.Exception("AnimatorStateEvent not found");

                var states = animatorStateEvent.GetAllStateName();
                for (int i = 0; i < states.Count; i++)
                {
                    Debug.Log($"State: {states[i]}");
                    int hashId = Utility.StringToHashId(states[i]);
                    animationHashMap.Add(hashId, new SpineMecanimAnimation(states[i], skeletonMecanim));
                }

                // foreach (Spine.Animation animation in skeletonMecanim.Skeleton.Data.Animations)
                // {
                //     // このanimationはSpineの元データでありMecanimのステートではない
                //     // Debug.Log("Animation name: " + animation.Name);
                //     // animations.Add(animation.Name, new SpineMecanimAnimation(skeletonMecanim, animation));

                //     // #if UNITY_EDITOR
                //     //                     // AnimatorControllerのステート存在チェック
                //     //                     if (states.Get(animation.Name) == null)
                //     //                     {
                //     //                         Debug.LogWarning($"Animator: AnimationState not found. name={animation.Name}");
                //     //                     }
                //     // #endif
                // }

                // コールバック
                // FIXME: 終了処理
                // skeletonMecanim.AnimationState.Event += OnHandleEvent;
                var detector = skeletonMecanim.gameObject.GetComponent<SpineMecanimAnimationEventDetector>();
                if (detector != null)
                {
                    detector.Setup(this);
                }
            }

            {
                // playableGraph.Play();
            }
        }

        public override async UniTask<int> InitializeAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(skeletonMecanim.gameObject.activeInHierarchy == true, "アクティブになっていません");

            // Animator animator = skeletonMecanim.GetComponent<Animator>();
            // if (animator == null) throw new System.Exception("Animator component not found");

            // AnimatorHelper
            // {
            //     var animatorHelper = skeletonMecanim.gameObject.AddComponent<AnimatorHelper>();
            //     if (animatorHelper == null) throw new System.Exception();
            //     // Awake()を待つ
            //     await UniTask.Yield(cancellationToken: cancellationToken);
            //     if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Canceled;
            // }

            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        public override void Dispose()
        {
            // playableGraph.Destroy();
        }

        public override void Restore()
        {
        }


        public override IAWTrack? SetAnimation(int hashId, bool loop = false, int trackNum = 0)
        {
            animator.Play(hashId, layer: trackNum);

            var track = trackList[trackNum];
            // track.Set(animation as SpineSkeletonAnimation);
            return track;
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

            // Animator animator = skeletonMecanim.GetComponent<Animator>();
            // Debug.Log(animation.Name);

            // SpineMecanimAnimation spineMecanimAnimation = (SpineMecanimAnimation)animation;
            // if (spineMecanimAnimation != null
            //     && spineMecanimAnimation.animationClip != null)
            // {
            //     // MEMO: AnimationClipのLoop Timeはfalseになっている前提
            //     double value = loop ? double.PositiveInfinity : (double)spineMecanimAnimation.animationClip.length;
            //     Debug.Log($"SetAnimation: {animation.Name}, loop={loop}, duration={value}");
            //     spineMecanimAnimation.animationClipPlayable.SetDuration(value);
            //     // playableGraph.Play();
            // }

            // MEMO: stateNameが存在しない場合は警告ログが出だだけでPlay()メソッドでは検知できない
            string stateName = animation.Name;
            // if (loop == true)
            // {
            //     stateName = animation.Name + "_loop";
            // }
#if UNITY_EDITOR
            Debug.Assert(states.ContainsKey(stateName) == true, $"GetAnimation: Not found name={stateName}");
#endif
            animator.Play(stateName: stateName, layer: trackIndex);

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


        // void OnHandleEvent(TrackEntry trackEntry, Spine.Event spineEvent)
        // {
        //     var animation = trackList[trackEntry.TrackIndex].Animation;
        //     InvokeAnimationEvent(animation, eventDecoder.Decode(spineEvent));
        // }

        public void OnSpineEvent(string eventName, float eventTime, int intValue, float floatValue, string stringValue)
        {
            // FIXME:
            var animation = trackList[0].Animation;
            if (animation == null) return;

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