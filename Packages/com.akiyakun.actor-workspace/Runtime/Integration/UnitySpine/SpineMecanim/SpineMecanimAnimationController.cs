#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Spine.Unity;
using afl;

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
    public class SpineMecanimAnimationController : SpineAnimationControllerBase<SpineMecanimAnimation, SpineMecanimTrack>
    {
        public override IAWAnimationParameter AnimationParameter { get; protected set; }

        // FIXME:
        public override bool IsVisibility
        {
            get => skeletonMecanim.GetComponent<Renderer>().enabled;
            set => skeletonMecanim.GetComponent<Renderer>().enabled = value;
        }

        protected bool enableRootMotion;
        public override bool EnableRootMotion
        {
            get => enableRootMotion;
            set => SetEnableRootMotion(value);
        }
        // public override bool RootMotionStatus;
        public override bool ApplyRootMotionPositionX
        {
            get => skeletonMecanimRootMotion.transformPositionX;
            set => skeletonMecanimRootMotion.transformPositionX = value;
        }
        public override bool ApplyRootMotionPositionY
        {
            get => skeletonMecanimRootMotion.transformPositionY;
            set => skeletonMecanimRootMotion.transformPositionY = value;
        }


        SkeletonMecanim skeletonMecanim;
        Animator animator;
        SkeletonMecanimRootMotion skeletonMecanimRootMotion;
        AnimatorStateEvent animatorStateEvent;
        IAWAnimationEventDecoder<UnityEngine.AnimationEvent> animationEventDecoder;

        int currentStateNameHash = 0;

// #if UNITY_EDITOR
//         Dictionary<string, UnityEditor.Animations.AnimatorState> states = new();
// #endif

        public SpineMecanimAnimationController(SkeletonMecanim skeletonMecanim)
            : base(skeletonMecanim)
        {
            this.skeletonMecanim = skeletonMecanim;
            // Debug.Assert(skeletonMecanim != null);

            animator = skeletonMecanim.GetComponent<Animator>();
            if (animator == null) throw new System.Exception("Animator component not found");

            skeletonMecanimRootMotion = skeletonMecanim.GetComponent<SkeletonMecanimRootMotion>();
            if (skeletonMecanimRootMotion == null) throw new System.Exception("skeletonMecanimRootMotion component not found");

            AnimationParameter = new MecanimAnimationParameter(animator);
            animationEventDecoder = new SpineMecanimAnimationEventDecoder();

            // IAWAnimationのリストを作成
            // if (skeletonMecanim != null)
            {
                // FIXME: layer=0 しか対応してない
                animatorStateEvent = AnimatorStateEvent.Get(animator, 0);
                if (animatorStateEvent == null) throw new System.Exception("AnimatorStateEvent not found");

                var states = animatorStateEvent.GetStateInfoList();
                for (int i = 0; i < states.Count; i++)
                {
                    var state = states[i];
                    D.Log(CoreLogMask.Verbose, $"State: StateFullPath={state.StateFullPath}, StateName={state.StateName}, StateNameHash={state.StateNameHash}");
                    animationHashMap.Add(state.StateNameHash, new SpineMecanimAnimation(state, skeletonMecanim));
                }

                animatorStateEvent.GenerateHashMap();


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
                // skeletonAnimation.AnimationState.Complete += OnHandleComplete;
                animatorStateEvent.OnStateEntered += OnHandleEntered;
                animatorStateEvent.OnStateExited += OnHandleComplete;
            }

            {
                // playableGraph.Play();



                skeletonMecanim.Translator.OnClipApplied += (spineAnim, layerIndex, weight, time, lastTime, backward) =>
                {
                    // spineAnim.Name が Animator 上の AnimationClip 名と対応
                };
            }
        }

        public override void CreateTrack()
        {
            for (int i = 0; i < IAWTrack.MaxTrack; i++)
            {
                trackList.Add(new SpineMecanimTrack(i));
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

            // SpineMecanimの取得と初期化
            {
                var spineMecanim = skeletonMecanim.gameObject.GetComponent<SpineMecanim>();
                if (spineMecanim == null)
                {
                    Debug.Assert(false, $"SpineMecanim コンポーネントがありません、追加してください。\nname={skeletonMecanim.gameObject.name}");
                    return GeneralReturnCode.Failed;
                }

                // ExtraDataのセットアップ
                ExtraData = spineMecanim.SpineExtraData;
                if (ExtraData != null)
                {
                    SetupForExtraData(skeletonMecanim.transform, skeletonMecanim);
                }

            }

            // SpineMecanimAnimationEventDetector の初期化
            {
                var detector = skeletonMecanim.gameObject.GetComponent<SpineMecanimAnimationEventDetector>();
                // if (detector == null)
                // {
                //     detector = skeletonMecanim.gameObject.AddComponent<SpineMecanimAnimationEventDetector>();
                // }
                if (detector == null || detector.Initialize(this) == false)
                {
                    Debug.Assert(false, "SpineMecanimAnimationEventDetector Initialize failed");
                    return GeneralReturnCode.Failed;
                }
            }

            // RootMotionの設定
            {
                // SetEnableRootMotion(skeletonMecanimRootMotion.enabled);
                SetEnableRootMotion(true);

                // 初期状態を適用
                ApplyRootMotion(animatorStateEvent.ApplyRootMotionByDefault);
            }

            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        public override void Dispose()
        {
            // playableGraph.Destroy();
        }

        public override void Restore()
        {
        }


        public override void SetEmptyAnimation(AWAnimationOption option = default)
        {
        }

        /*
            option.Immediate == true のとき
                対象のステートを直接再生します。
            option.Immediate == false のとき
                ステートと同名のInt型パラメータを1に設定します。
         */
        public override IAWTrack? SetAnimation(int nameHash, AWAnimationOption option = default)
        {
            SpineMecanimAnimation? animationImpl = GetAnimationImpl(nameHash);
            if (animationImpl == null)
            {
                Debug.Assert(false, $"Cast error.");
                return null;
            }

            // Debug.Log($"SetAnimation: Name={animationImpl.Name}, Immediate={option.Immediate}, Track={option.Track}");

            // AnimationSetting(animationImpl);

            if (option.Immediate == true)
            {
                animator.Play(nameHash, layer: option.Track);
            }
            // if (option.Parameter == true)
            else
            {
                // ステートと同名のInt型パラメータを1に設定します
                // animator.SetInteger(animationImpl.stateOptionInfo.StateNameHash, 1);
                AnimationParameter.SetInt(animationImpl.stateOptionInfo.StateNameHash, 1);
                // return trackList[option.Track];
            }

            var track = trackList[option.Track];
            track.Set(animationImpl);
            return track;
        }

        /*
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
        */

#if false
        public override IAWTrack? SetAnimation(int trackIndex, IAWAnimation animation, bool loop)
        {
            if (animation == null)
            {
                Debug.Assert(false, $"SetAnimation: trackIndex={trackIndex}, animation={animation?.Name}, loop={loop}");
                return null;
            }

            // Animator animator = skeletonMecanim.GetComponent<Animator>();
            // Debug.Log(animation.Name);

            SpineMecanimAnimation? spineMecanimAnimation = animation as SpineMecanimAnimation;
            if (spineMecanimAnimation == null)
            {
                Debug.Assert(false, $"Cast error.");
                return null;
            }
            // if (spineMecanimAnimation != null
            //     && spineMecanimAnimation.animationClip != null)
            // {
            //     // MEMO: AnimationClipのLoop Timeはfalseになっている前提
            //     double value = loop ? double.PositiveInfinity : (double)spineMecanimAnimation.animationClip.length;
            //     Debug.Log($"SetAnimation: {animation.Name}, loop={loop}, duration={value}");
            //     spineMecanimAnimation.animationClipPlayable.SetDuration(value);
            //     // playableGraph.Play();
            // }

            if (spineMecanimAnimation.stateOptionInfo.HasOptionFlag(AnimatorStateOptionFlag.NoRootMotion))
            {
                animator.applyRootMotion = false;
                skeletonMecanimRootMotion.enabled = false;
                Debug.Log($"RootMotion: disable");
            }
            else
            {
                // FIXME: とりま今は基本RootMotion有効で動かしている
                animator.applyRootMotion = true;
                skeletonMecanimRootMotion.enabled = true;
                Debug.Log($"RootMotion: enable");
            }

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

            {
                /*
                var skel = GetComponent<SkeletonMecanim>();
                var animator = skel.Translator.Animator;

                int layer = 0;
                var currentInfos = animator.GetCurrentAnimatorClipInfo(layer);   // 現在ステート
                var nextInfos    = animator.GetNextAnimatorClipInfo(layer);      // 遷移先ステート（遷移中のみ）

                foreach (var info in currentInfos) {
                    AnimationClip clip = info.clip;
                    // 使用中クリップ
                }
                foreach (var info in nextInfos) {
                    AnimationClip clip = info.clip;
                    // 遷移先クリップ
                }
                */

                int layer = 0;
                var animator = skeletonMecanim.Translator.Animator;
                var currentInfos = animator.GetCurrentAnimatorClipInfo(layer);

            }

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
#endif

        // public override void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f)
        // {
        //     // skeletonAnimation.state.AddAnimation(trackIndex, animation.Name, loop: false, delay: delay);
        // }

        public override IAWAnimation? GetCurrentAnimation(int track = 0)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(track);
            return GetAnimation(currentState.shortNameHash);
        }

        public override bool IsPlayingAnimation(int nameHash, int track = 0)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(track);
            return currentState.shortNameHash == nameHash;
        }

        // public static bool IsPlayingState(Animator animator, string stateName, int layerIndex = 0)
        // {
        //     // 現在のステートをチェック
        //     AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(layerIndex);
        //     if (currentState.IsName(stateName))
        //         return true;

        //     // トランジション中の次のステートもチェック
        //     if (animator.IsInTransition(layerIndex))
        //     {
        //         AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(layerIndex);
        //         if (nextState.IsName(stateName))
        //             return true;
        //     }

        //     return false;
        // }

        public override IAWTrack? GetTrack(int trackIndex)
        {
            // TrackEntry cu = skeletonAnimation.AnimationState.GetCurrent(trackIndex);
            // if (cu == null) return null;
            // trackList[trackIndex].TrackEntry = cu;
            return trackList[trackIndex];
        }


        // Mecanimのステートに入ったときのコールバック
        void OnHandleEntered(AnimatorStateOptionInfo info)
        {
            // Debug.Log($"OnHandleEntered: State={info.StateName}, RootMotion={info.HasOptionFlag(AnimatorStateOptionFlag.RootMotion)}");

            // MEMO: 1Frame目のイベントが実行されている場合ここで抜けることになる
            if (currentStateNameHash == info.StateNameHash) return;

            var animation = GetAnimationImpl(info.StateNameHash);
            if (animation == null) return;

            currentStateNameHash = info.StateNameHash;

            // FIXME: パラメータリセットはどこのタイミングでやるべきか・・・
            // とりあえず自身のパラメータは遷移したらすぐにリセットが必要無きがする
            // AnimationParameter.SetInt(info.StateName, 0);

            // AnimationSetting(animation);

            // Tag:RootMotion
            // if (EnableRootMotion == false
            //     && animation.stateOptionInfo.HasOptionFlag(AnimatorStateOptionFlag.RootMotion))
            // {
            //     EnableRootMotion = true;
            //     // Debug.Log($"RootMotion: Enable name={animation.Name}");
            // }
            if (EnableRootMotion == true)
            {
                if (RootMotionStatus == false
                    && animation.stateOptionInfo.HasOptionFlag(AnimatorStateOptionFlag.RootMotion))
                {
                    ApplyRootMotion(true);
                    // Debug.Log($"RootMotion: Enable name={animation.Name}");
                }
            }

            // // 遷移中でないとき
            // if (animator.IsInTransition(0) == false)
            // {
            //     if (animation.stateOptionInfo.HasOptionFlag(AnimatorStateOptionFlag.NoRootMotion))
            //     {
            //         // if (animator.applyRootMotion == true)
            //         // {
            //         //     skeletonMecanimRootMotion.rigidBody2D.transform.position += delta;
            //         // }

            //         // var before = skeletonMecanimRootMotion.rigidBody2D.transform.position;
            //         Vector3 delta = animator.deltaPosition;

            //         animator.applyRootMotion = false;
            //         skeletonMecanimRootMotion.enabled = false;

            //         // skeletonMecanimRootMotion.rigidBody2D.transform.position = before;
            //         skeletonMecanimRootMotion.rigidBody2D.transform.position += delta;

            //         Debug.Log($"RootMotion: Disable name={animation.Name}, delta={delta}");
            //     }
            // }

            // FIXME:
            var track = trackList[0];
            track.Set(animation);

            InvokeAnimationEntered(animation);

        }

        // Mecanimのステートが完了したときのコールバック
        void OnHandleComplete(AnimatorStateOptionInfo info)
        {
            // Debug.Log($"OnHandleComplete: State={info.StateName}, RootMotion={info.HasOptionFlag(AnimatorStateOptionFlag.RootMotion)}");
            var animation = GetAnimationImpl(info.StateNameHash);
            if (animation == null) return;

            // 遷移中のとき
            // クロスフェード中のみ？
            // if (animator.IsInTransition(0))
            AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);
            // Debug.Log($"next name={nextInfo.shortNameHash}");

            AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
            // Debug.Log($"current name={currentInfo.shortNameHash}");

            // MEMO: Nextがあるときの判定はこれでいいのだろうか・・・
            // if (nextInfo.shortNameHash != 0)
            if (currentInfo.shortNameHash != 0)
            {
                nextInfo = currentInfo;
                var nextAnimation = GetAnimationImpl(nextInfo.shortNameHash)!;

                // Debug.Log($"遷移中 next name={nextAnimation.Name}");

                // Tag:RootMotion
                // if (nextAnimation!.stateOptionInfo.HasOptionFlag(AnimatorStateOptionFlag.NoRootMotion))
                // {
                //     EnableRootMotion = false;
                //     // Debug.Log($"RootMotion: Disable name={animation.Name}");
                // }
                // if (EnableRootMotion == true)
                {
                    if (RootMotionStatus == true
                        && nextAnimation.stateOptionInfo.HasOptionFlag(AnimatorStateOptionFlag.NoRootMotion))
                    {
                        ApplyRootMotion(false, force: true);
                        // Debug.Log($"RootMotion: Enable name={animation.Name}");
                    }
                }
            }
            else
            {
                // Debug.Log("遷移してない");
                AnimationSetting(animation);
            }


            InvokeAnimationComplete(animation);
        }

        // void OnHandleEvent(TrackEntry trackEntry, Spine.Event spineEvent)
        // {
        //     var animation = trackList[trackEntry.TrackIndex].Animation;
        //     InvokeAnimationEvent(animation, eventDecoder.Decode(spineEvent));
        // }

        // public void OnSpineEvent(string eventName, float eventTime, int intValue, float floatValue, string stringValue)
        // {
        //     // FIXME:
        //     var animation = trackList[0].Animation;
        //     if (animation == null) return;

        //     AWEventData eventData = new AWEventData
        //     {
        //         Name = eventName,
        //         Int = intValue,
        //         Float = floatValue,
        //         String = stringValue,
        //     };
        //     // InvokeAnimationEvent(animation, eventDecoder.Decode(eventName));
        //     InvokeAnimationEvent(animation, eventData);
        // }
        public void OnSpineEvent(AnimationEvent rawData)
        {
            // FIXME:
            var animation = trackList[0].Animation;
            if (animation == null) return;

            // OnHandleEntered()より先にイベントが呼ばれた場合ここに来る
            // イベント処理よりも先に OnHandleEntered() の処理をしたいので明示的に呼び出す
            if (rawData.animatorStateInfo.shortNameHash != currentStateNameHash)
            {
                // D.Log($"OnSpineEvent: OnHandleEntered()より先にイベントが呼ばれた");
                AnimatorStateOptionInfo info = animatorStateEvent.GetStateInfo(rawData.animatorStateInfo.fullPathHash);
                OnHandleEntered(info);
            }

            /*
            var d = animationEventDecoder.Decode(rawData);
            Debug.Log($"OnSpineEvent: name={d.Name}, int={d.Int}, float={d.Float}, string={d.String}");
            //*/

            InvokeAnimationEvent(animation, animationEventDecoder.Decode(rawData));
        }

        void AnimationSetting(SpineMecanimAnimation animation)
        {
            if (animation.stateOptionInfo.HasOptionFlag(AnimatorStateOptionFlag.NoRootMotion))
            {
                ApplyRootMotion(false);
                // Debug.Log($"RootMotion: Disable name={animation.Name}, delta.x={animator.deltaPosition}");
            }
            else
            {
                // FIXME: とりま今は基本RootMotion有効で動かしている
                ApplyRootMotion(true);
                // Debug.Log($"RootMotion: Enable name={animation.Name}");
            }
        }

        void SetEnableRootMotion(bool enable)
        {
            enableRootMotion = enable;
            ApplyRootMotion(enable, force: true);
        }

        void ApplyRootMotion(bool enable, bool force = false)
        {
            // Tag:RootMotion

            // RootMotionが無効なときは何もしない
            if (force == false && EnableRootMotion == false) return;

            // Debug.Log($"ApplyRootMotion: Enable={enable}, force={force}");

            if (enable == true)
            {
                animator.applyRootMotion = true;
                skeletonMecanimRootMotion.enabled = true;
                RootMotionStatus = true;
            }
            else
            {
                animator.applyRootMotion = false;
                skeletonMecanimRootMotion.enabled = false;
                RootMotionStatus = false;

                // var before = skeletonMecanimRootMotion.rigidBody2D.transform.position;
                Vector3 delta = animator.deltaPosition;
                // Vector3 delta = accumulatedDeltaPosition;

                animator.applyRootMotion = false;
                skeletonMecanimRootMotion.enabled = false;

                // skeletonMecanimRootMotion.rigidBody2D.transform.position = before;
                skeletonMecanimRootMotion.rigidBody2D.transform.position += delta;

                // リセット
                // accumulatedDeltaPosition = Vector3.zero;

                // Debug.Log($"RootMotion: Disable delta.x={animator.deltaPosition}");
            }
        }

        // Vector3 accumulatedDeltaPosition = Vector3.zero;
        // public override void DoUpdate(float deltaTime)
        // {
        //     // 毎フレーム deltaPosition を積算
        //     Vector3 delta = animator.deltaPosition;
        //     accumulatedDeltaPosition += delta;

        //     Debug.Log($"DoUpdate: deltaPosition={delta.x}, accumulated={accumulatedDeltaPosition.x}");
        // }
    }
}
#nullable restore