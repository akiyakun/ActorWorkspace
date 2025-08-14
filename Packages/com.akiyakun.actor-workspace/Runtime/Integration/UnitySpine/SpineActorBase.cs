#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Spine.Unity;
using afl;

namespace ActorWorkspace.UnitySpine
{
    public class SpineActorBase<TActorContextProvider, TActorParam, TActorDisplay>
        : AWActorBase<TActorContextProvider, TActorParam, TActorDisplay, SpineAnimationController, SpineSkin>
        where TActorContextProvider : AWActorContextProvider
        where TActorParam : class, IAWActorParam, new()
        where TActorDisplay : SpineActorDisplay, new()
    {
        public override TActorContextProvider ActorContextProvider { get; protected set; } = null!;

        public override TActorParam ActorParam { get; protected set; } = new();
        public override TActorDisplay ActorDisplay { get; protected set; } = null!;
        public override SpineAnimationController AnimationController { get; protected set; } = null!;
        public override IReadOnlyList<SpineSkin> SkinList => skinList;

        protected Spine.Skeleton Skeleton => skeletonAnimationInterface.Skeleton;

        ISkeletonAnimation skeletonAnimationInterface = null!;
        IAWEventDecoder eventDecoder = null!;
        // SpineSkeletonAnimationController spineSkeletonAnimationController = null!;
        List<SpineSkin> skinList = new();

        protected override async UniTask<int> InnerInitializeAsync(AWActorContextProvider awActorContextProvider, CancellationToken cancellationToken)
        {
            ActorContextProvider = (TActorContextProvider)awActorContextProvider;
            if (ActorContextProvider == null) return GeneralReturnCode.Failed;

            // SkeletonMecanim か SkeletonAnimation のどちらかを取得
            if (GetComponentInChildren<SkeletonMecanim>(includeInactive: false) is SkeletonMecanim skeletonMecanim)
            {
                int ret = await InitializeSkeletonMecanimAsync(skeletonMecanim, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Cancel;
                if (ret < 0) return ret;
            }
            else if (GetComponentInChildren<SkeletonAnimation>(includeInactive: false) is SkeletonAnimation skeletonAnimation)
            {
                if (GetComponentInChildren<SkeletonAnimation>(includeInactive: false) is not null) { }

                int ret = await InitializeSkeletonAnimationAsync(skeletonAnimation, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Cancel;
                if (ret < 0) return ret;
            }
            else
            {
                Debug.Assert(false, "SkeletonMecanim or SkeletonAnimation not found");
                return GeneralReturnCode.Failed;
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
            {
                ActorDisplay = new TActorDisplay();
                int ret = await ActorDisplay.InitializeAsync(AnimationController, cancellationToken: cancellationToken);
                if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Cancel;
                if (ret < 0) return ret;
            }

            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        protected virtual async UniTask<int> InitializeSkeletonMecanimAsync(SkeletonMecanim skeletonMecanim, CancellationToken cancellationToken)
        {
            skeletonAnimationInterface = skeletonMecanim;
            Debug.Assert(skeletonAnimationInterface != null);

            // FIXME: SkeletonMecanim用のが必要
            eventDecoder = new SpineEventDecoder();
            // Debug.Assert(eventDecoder != null);

            var spineMecanimAnimationController = new SpineMecanimAnimationController(skeletonMecanim, eventDecoder);
            AnimationController = spineMecanimAnimationController as SpineAnimationController;
            Debug.Assert(AnimationController != null);

            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        protected virtual async UniTask<int> InitializeSkeletonAnimationAsync(SkeletonAnimation skeletonAnimation, CancellationToken cancellationToken)
        {
            // skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            // if (skeletonAnimation == null) return GeneralReturnCode.Failed;

            skeletonAnimationInterface = skeletonAnimation;
            Debug.Assert(skeletonAnimationInterface != null);

            eventDecoder = new SpineEventDecoder();
            // Debug.Assert(eventDecoder != null);

            var spineSkeletonAnimationController = new SpineSkeletonAnimationController(skeletonAnimation, eventDecoder);
            AnimationController = spineSkeletonAnimationController as SpineAnimationController;
            Debug.Assert(AnimationController != null);

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