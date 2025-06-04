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
            // actorAssetInfoList.Add(new ActorAssetInfoEx { Name = "Actor1" });
            // actorAssetInfoList.Add(new ActorAssetInfoEx { Name = "Actor2" });
            // actorAssetInfoList.Add(new ActorAssetInfoEx { Name = "Actor3" });

            var targetFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(
                "Assets/SpineData");
            IReadOnlyList<SkeletonDataAsset> result =
                EUtility.FindAssets<SkeletonDataAsset>(targetFolder);
            for (int i = 0; i < result.Count; i++)
            {
                var skeletonDataAsset = result[i];

                var info = new ActorAssetInfoEx();
                info.Name = skeletonDataAsset.name;
                info.Path = AssetDatabase.GetAssetPath(skeletonDataAsset);
                Debug.Log($"Found SkeletonDataAsset: {info.Name} at {info.Path}");
                actorAssetInfoList.Add(info);
            }


            // {
            //     var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(
            //         "Assets/SpineData/Player/Player_SkeletonData.asset");
            //     var newSkeleton = new GameObject("Player");
            //     var sanim = newSkeleton.AddComponent<SkeletonAnimation>();
            //     sanim.skeletonDataAsset = skeletonDataAsset;
            //     sanim.Initialize(true);
            // }
        }

        public List<ActorAssetInfo> GetAll()
        {
            return actorAssetInfoList;
        }
    }
}
