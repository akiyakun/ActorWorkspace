#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Spine.Unity;
using afl;

namespace ActorWorkspace.UnitySpine
{
    public abstract class SpineActorBase<TActorParam>
        : AWActorBase<TActorParam, IAWAnimationController, SpineSkin>
        // where TActorContextProvider : AWActorContextProvider
        where TActorParam : class, IAWActorParam
        // where TActorDisplay : SpineActorDisplay, new()
    {
        // public override TActorContextProvider ActorContextProvider { get; protected set; } = null!;

        // public override TActorParam ActorParam { get; protected set; }
        // public override TActorDisplay ActorDisplay { get; protected set; } = null!;
        // public override IAWAnimationController AnimationController { get; protected set; } = null!;
        public override IReadOnlyList<SpineSkin> SkinList => skinList;

        protected Spine.Skeleton Skeleton => skeletonAnimationInterface.Skeleton;

        ISkeletonAnimation skeletonAnimationInterface = null!;
        // SpineSkeletonAnimationController spineSkeletonAnimationController = null!;
        List<SpineSkin> skinList = new();

        protected override async UniTask<int> CreateAnimationController(CancellationToken cancellationToken)
        {
            // SkeletonMecanim か SkeletonAnimation のどちらかを取得
            if (GetComponentInChildren<SkeletonMecanim>(includeInactive: false) is SkeletonMecanim skeletonMecanim)
            {
                skeletonAnimationInterface = skeletonMecanim;
                Debug.Assert(skeletonAnimationInterface != null);

                var spineMecanimAnimationController = new SpineMecanimAnimationController(skeletonMecanim);
                AnimationController = spineMecanimAnimationController as IAWAnimationController;
            }
            else if (GetComponentInChildren<SkeletonAnimation>(includeInactive: false) is SkeletonAnimation skeletonAnimation)
            {
                skeletonAnimationInterface = skeletonAnimation;
                Debug.Assert(skeletonAnimationInterface != null);

                var spineSkeletonAnimationController = new SpineSkeletonAnimationController(skeletonAnimation);
                AnimationController = spineSkeletonAnimationController as IAWAnimationController;
            }
            else
            {
                Debug.Assert(false, "SkeletonMecanim or SkeletonAnimation not found");
                return GeneralReturnCode.Failed;
            }

            if (AnimationController == null)
            {
                Debug.Assert(false, "Failed to create AnimationController");
                return GeneralReturnCode.Failed;
            }

            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        protected override async UniTask<int> InnerInitializeAsync(CancellationToken cancellationToken)
        {
            // ActorContextProvider = (TActorContextProvider)awActorContextProvider;
            // if (ActorContextProvider == null) return GeneralReturnCode.Failed;

            // AnimationControllerの初期化
            {
                if (AnimationController is SpineMecanimAnimationController mecanim)
                {
                    if (await mecanim.InitializeAsync(cancellationToken) is int ret && ret < 0) return ret;
                }
                else if (AnimationController is SpineSkeletonAnimationController skeleton)
                {
                    if (await skeleton.InitializeAsync(cancellationToken) is int ret && ret < 0) return ret;
                }
                else
                {
                    Debug.Assert(false, "Invalid AnimationController type");
                    return GeneralReturnCode.Failed;
                }
            }

            // スキンの初期化
            var skins = Skeleton.Data.Skins.Items;
            // skinList = new List<SpineSkin>(skins.Length);
            for (int i = 0; i < skins.Length; i++)
            {
                var skin = new SpineSkin(skins[i]);
                skinList.Add(skin);
            }

            // ActorDisplayの初期化
            // {
            //     ActorDisplay = new TActorDisplay();
            //     int ret = await ActorDisplay.InitializeAsync(this, AnimationController, cancellationToken: cancellationToken);
            //     if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Canceled;
            //     if (ret < 0) return ret;
            // }

            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        // public void Restore()
        // {
        //     ActorParam?.Restore();
        // }

        public override void SetSkin(int skinIndex)
        {
            base.SetSkin(skinIndex);

            if (skinIndex < 0 || skinList.Count <= skinIndex)
            {
                Debug.Assert(false, $"Invalid skin index: {skinIndex}");
                return;
            }

            var skin = skinList[skinIndex];

            Skeleton.SetSkin(skin.Skin);
            Skeleton.SetSlotsToSetupPose();
        }
    }
}
#nullable restore