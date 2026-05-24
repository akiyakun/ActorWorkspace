#nullable enable
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using afl.MasterData;
using Spine.Unity;

#if UNITY_EDITOR
using UnityEditor;

namespace ActorWorkspace.UnitySpine
{
    // AssetDatabaseからFakeSpineActorを生成
    public class SpineActorFactoryInAssetDatabase : IAWActorFactory
    {
        public event System.Action<IAWActor>? OnCreated;
        public event System.Action<IAWActor>? OnRelease;

        IAssetRepository assetRepository;

#nullable disable
        private SpineActorFactoryInAssetDatabase() {}
#nullable enable

        public SpineActorFactoryInAssetDatabase(IAssetRepository assetRepository)
        {
            this.assetRepository = assetRepository;
            Debug.Assert(assetRepository != null);
        }

        // From IActorFactory
        public virtual async UniTask<IAWActor?> CreateAsync(ActorCreateParam param, CancellationToken cancellationToken = default)
        {
            await UniTask.Yield(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return null;

            // Debug.Assert((uint)id < (uint)masterData.Length);
            var model = assetRepository.GetAssetModel(param.Id);
            if (model == null) throw new System.Exception($"AssetModel not found for id: {param.Id}");
            string locator = model.AssetLocator;
            // Debug.Log($"SpineActorFactoryInAssetDatabase.CreateAsync(): {locator}");
            // var skeletonAnimation = SpineUtility.CreateSkeletonAnimationFromAssetDatabae(locator);
            var skeletonAnimation = CreateSkeletonAnimationFromAssetDatabae(locator);

            var spineSkeletonActor = skeletonAnimation.gameObject.AddComponent<RawSpineActor>();
            IAWActor actor = spineSkeletonActor as IAWActor;
            if (actor == null) throw new System.Exception("Failed to create IAWActor");

            OnCreated?.Invoke(actor);

            return actor;
        }

        // From IActorFactory
        // public virtual IAWActor? Create(ActorCreateParam param)
        // {
        //     var task = CreateAsync(param, default).AsTask();
        //     task.Wait();
        //     return task.Result;
        // }

        public static SkeletonAnimation CreateSkeletonAnimationFromAssetDatabae(string path)
        {
            var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
            if (skeletonDataAsset == null) throw new System.Exception($"SkeletonDataAsset not found at path: {path}");
            var newSkeleton = new GameObject(skeletonDataAsset.name);
            var skeletonAnimation = newSkeleton.AddComponent<SkeletonAnimation>();
            skeletonAnimation.skeletonDataAsset = skeletonDataAsset;
            skeletonAnimation.Initialize(true);

            // skeletonAnimation.skeleton.SetSkin(skins.Items[skinIndex]);
            skeletonAnimation.Skeleton.SetSlotsToSetupPose();

            return skeletonAnimation;
        }

        public bool Release(IAWActor actor)
        {
            return true;
        }

    }
}
#endif
#nullable restore