using UnityEngine;
using Spine;
using Spine.Unity;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SpineHelper
{
#if UNITY_EDITOR
    public static GameObject CreateSkeletonGameObjectFromAsset(string path)
    {
        var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
        Debug.Assert(skeletonDataAsset != null, $"SkeletonDataAsset not found at path: {path}");
        var newSkeleton = new GameObject(skeletonDataAsset.name);
        var skeletonAnimation = newSkeleton.AddComponent<SkeletonAnimation>();
        skeletonAnimation.skeletonDataAsset = skeletonDataAsset;
        skeletonAnimation.Initialize(true);

        // skeletonAnimation.skeleton.SetSkin(skins.Items[skinIndex]);
        skeletonAnimation.Skeleton.SetSlotsToSetupPose();

        return newSkeleton;
    }
#endif
}
