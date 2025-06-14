#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using ActorWorkspace.InAppDebug;
using afl;
using Spine.Unity;

using UnityEditor;
using afl.Editor;

namespace Project.InAppDebug.Editor
{
    public class ActorAssetDatabaseInEditor : IActorAssetDatabase
    {
        List<ActorAssetInfo> actorAssetInfoList = new();

        public ActorAssetDatabaseInEditor()
        {
            var targetFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(
                "Assets/AssetData/SpineData");
            IReadOnlyList<SkeletonDataAsset> result =
                EUtility.FindAssets<SkeletonDataAsset>(targetFolder);
            for (int i = 0; i < result.Count; i++)
            {
                var skeletonDataAsset = result[i];

                var info = new ActorAssetInfo();
                info.Name = skeletonDataAsset.name;
                info.Path = AssetDatabase.GetAssetPath(skeletonDataAsset);
                // Debug.Log($"Found SkeletonDataAsset: {info.Name} at {info.Path}");
                actorAssetInfoList.Add(info);
            }

        }

        public List<ActorAssetInfo> GetAll()
        {
            return actorAssetInfoList;
        }

        public GameObject CreateActorAsset(string path)
        {
            return SpineHelper.CreateSkeletonAnimationFromAssetDatabae(path).gameObject;
        }
    }
}
#endif
