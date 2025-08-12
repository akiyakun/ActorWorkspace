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

        SkeletonAnimation skeletonAnimation = null!;
        IAWEventDecoder eventDecoder = null!;
        SpineSkeletonAnimationController spineSkeletonAnimationController = null!;
        List<SpineSkin> skinList = new();

        protected override async UniTask<int> InnerInitializeAsync(AWActorContextProvider awActorContextProvider, CancellationToken cancellationToken)
        {
            ActorContextProvider = (TActorContextProvider)awActorContextProvider;
            if (ActorContextProvider == null) return GeneralReturnCode.Failed;

            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            if (skeletonAnimation == null) return GeneralReturnCode.Failed;

            eventDecoder = new SpineEventDecoder();
            Debug.Assert(eventDecoder != null);

            {
                // var skeletonMecanim = GetComponent<SkeletonMecanim>();
                // var skeletonAnimation = GetComponent<SkeletonAnimation>();
                ActorDisplay = new TActorDisplay();
                int ret = await ActorDisplay.InitializeAsync(skeletonAnimation, cancellationToken: cancellationToken);
                if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Cancel;
                if (ret < 0) return ret;
            }

            spineSkeletonAnimationController = new SpineSkeletonAnimationController(skeletonAnimation, eventDecoder);
            AnimationController = spineSkeletonAnimationController as SpineAnimationController;

            var skins = skeletonAnimation.Skeleton.Data.Skins.Items;
            // skinList = new List<SpineSkin>(skins.Length);
            for (int i = 0; i < skins.Length; i++)
            {
                var skin = new SpineSkin(skins[i]);
                skinList.Add(skin);
            }

            ActorBehaviourController = new AWActorBehaviourController(this);

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

            var skeleton = skeletonAnimation.Skeleton;
            skeleton.SetSkin(skin.Skin);
            skeleton.SetSlotsToSetupPose();
        }
    }
}
#nullable restore