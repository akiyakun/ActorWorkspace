using System.Collections.Generic;
using UnityEngine;
using afl;
using ActorWorkspace.ActorAssetDatabase;
using ActorWorkspace.ActorAssetDatabase.Editor;
using Spine.Unity;

using UnityEditor;
using afl.Editor;

namespace ActorWorkspace.Editor.UnitySpine.ActorAssetDatabase
{
    public class SpineActorAssetDatabaseForEditor : ActorAssetDatabaseInEditor
    {
        protected override void Awake()
        {
            base.Awake();

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

        public override GameObject CreateActorAsset(string path)
        {
            return SpineUtility.CreateSkeletonAnimationFromAssetDatabae(path).gameObject;
        }
    }
}
