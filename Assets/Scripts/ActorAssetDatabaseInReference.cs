using System.Collections.Generic;
using UnityEngine;
using ActorWorkspace.InAppDebug;
using afl.MasterData;

using Spine.Unity;

namespace Project.InAppDebug
{
    public class ActorAssetDatabaseInReference : MonoBehaviour, IActorAssetDatabase
    {
        [SerializeField] ObjectReferenceScriptableObject objectReferences;
        List<ActorAssetInfo> actorAssetInfoList = new();

        // public ActorAssetDatabaseInReference()
        void Awake()
        {
            //     var targetFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(
            //         "Assets/AssetData/SpineData");
            //     IReadOnlyList<SkeletonDataAsset> result =
            //         EUtility.FindAssets<SkeletonDataAsset>(targetFolder);
            //     for (int i = 0; i < result.Count; i++)
            //     {
            //         var skeletonDataAsset = result[i];

            //         var info = new ActorAssetInfoEx();
            //         info.Name = skeletonDataAsset.name;
            //         info.Path = AssetDatabase.GetAssetPath(skeletonDataAsset);
            //         // Debug.Log($"Found SkeletonDataAsset: {info.Name} at {info.Path}");
            //         actorAssetInfoList.Add(info);
            //     }

            foreach (var data in objectReferences.GetList())
            {
                var info = new ActorAssetInfo();
                info.Name = data.Name;
                info.Path = data.Name;
                actorAssetInfoList.Add(info);
            }
        }

        public List<ActorAssetInfo> GetAll()
        {
            return actorAssetInfoList;
        }

        public GameObject CreateActorAsset(string path)
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

        UnityEngine.Object GetActorAssetInfo(string path)
        {
            foreach (var data in objectReferences.GetList())
            {
                if (data.Name == path)
                {
                    return data.Object;
                }
            }

            return null;
        }
    }
}
