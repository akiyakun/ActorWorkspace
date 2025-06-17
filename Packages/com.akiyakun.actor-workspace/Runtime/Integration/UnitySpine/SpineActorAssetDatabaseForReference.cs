using System.Collections.Generic;
using UnityEngine;
using ActorWorkspace.ActorAssetDatabase;

using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    public class SpineActorAssetDatabaseForReference : ActorAssetDatabaseForReference
    {
        public override GameObject CreateActorAsset(string path)
        {
            var original = GetActorAssetInfo(path);
            if (original == null)
            {
                Debug.LogError($"Actor asset not found at path: {path}");
                return null;
            }

            var skeletonDataAsset = GameObject.Instantiate(original) as SkeletonDataAsset;

            // var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
            Debug.Assert(skeletonDataAsset != null, $"SkeletonDataAsset not found at path: {path}");
            var newSkeleton = new GameObject(skeletonDataAsset.name);
            var skeletonAnimation = newSkeleton.AddComponent<SkeletonAnimation>();
            skeletonAnimation.skeletonDataAsset = skeletonDataAsset;
            skeletonAnimation.Initialize(true);

            // skeletonAnimation.skeleton.SetSkin(skins.Items[skinIndex]);
            skeletonAnimation.Skeleton.SetSlotsToSetupPose();

            return skeletonAnimation.gameObject;
        }
    }
}
