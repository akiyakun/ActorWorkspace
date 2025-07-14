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
    // AssetDatabaseからSpineのアセットを生成
    public class SpineActorFactoryInAssetDatabase : IAWActorFactory
    {
        public event System.Action<IAWActor> OnCreated;

        IAssetRepository assetRepository;

        private SpineActorFactoryInAssetDatabase()
        {
        }

        public SpineActorFactoryInAssetDatabase(IAssetRepository assetRepository)
        {
            Debug.Assert(assetRepository != null);
            this.assetRepository = assetRepository;
        }

        // From IActorFactory
        public virtual async UniTask<IAWActor> CreateAsync(int id, int category = 0, CancellationToken cancellationToken = default)
        {
            await UniTask.Yield(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return null;

            // Debug.Assert((uint)id < (uint)masterData.Length);
            var model = assetRepository.GetAssetModel(id);
            Debug.Assert(model != null, $"AssetModel not found for id: {id}");
            string locator = model.AssetLocator;
            // Debug.Log($"SpineActorFactoryInAssetDatabase.CreateAsync(): {locator}");
            // var skeletonAnimation = SpineUtility.CreateSkeletonAnimationFromAssetDatabae(locator);
            var skeletonAnimation = CreateSkeletonAnimationFromAssetDatabae(locator);

            var spineSkeletonActor = skeletonAnimation.gameObject.AddComponent<SpineSkeletonActor>();
            IAWActor actor = spineSkeletonActor as IAWActor;
            Debug.Assert(actor != null);

            OnCreated?.Invoke(actor);

            return actor;
        }

        public static SkeletonAnimation CreateSkeletonAnimationFromAssetDatabae(string path)
        {
            var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
            Debug.Assert(skeletonDataAsset != null, $"SkeletonDataAsset not found at path: {path}");
            var newSkeleton = new GameObject(skeletonDataAsset.name);
            var skeletonAnimation = newSkeleton.AddComponent<SkeletonAnimation>();
            skeletonAnimation.skeletonDataAsset = skeletonDataAsset;
            skeletonAnimation.Initialize(true);

            // skeletonAnimation.skeleton.SetSkin(skins.Items[skinIndex]);
            skeletonAnimation.Skeleton.SetSlotsToSetupPose();

            return skeletonAnimation;
        }

    }
}
#endif