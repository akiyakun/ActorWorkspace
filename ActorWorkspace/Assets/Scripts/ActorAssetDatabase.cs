using System.Collections.Generic;
using UnityEngine;
using ActorWorkspace.InAppDebug;
using afl;
using Spine.Unity;

using UnityEditor;
// using Project.InAppDebug.Editor;
using afl.Editor;
using Unity.VisualScripting;

namespace Project.InAppDebug.Editor
{
    public class ActorAssetDatabase : IActorAssetDatabase
    {
        List<ActorAssetInfo> actorAssetInfoList = new();

        public ActorAssetDatabase()
        {
            var targetFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(
                "Assets/AssetData/SpineData");
            IReadOnlyList<SkeletonDataAsset> result =
                EUtility.FindAssets<SkeletonDataAsset>(targetFolder);
            for (int i = 0; i < result.Count; i++)
            {
                var skeletonDataAsset = result[i];

                var info = new ActorAssetInfoEx();
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
    }
}
