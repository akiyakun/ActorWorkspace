using UnityEngine;
using UnityEditor;
using Spine.Unity;

namespace ActorWorkspace.Editor.UnitySpine
{
    public static partial class SpineHelper
    {
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
