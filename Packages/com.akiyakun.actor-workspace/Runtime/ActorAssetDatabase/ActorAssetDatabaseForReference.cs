using System.Collections.Generic;
using UnityEngine;
using afl.MasterData;

namespace ActorWorkspace.ActorAssetDatabase
{
    public abstract class ActorAssetDatabaseForReference : MonoBehaviour, IActorAssetDatabase
    {
        [SerializeField] ObjectReferenceScriptableObject objectReferences;
        List<ActorAssetInfo> actorAssetInfoList = new();

        // public ActorAssetDatabaseInReference()
        protected void Awake()
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

        public abstract GameObject CreateActorAsset(string path);
        // public GameObject CreateActorAsset(string path)
        // {
        //     var original = GetActorAssetInfo(path);
        //     if (original == null)
        //     {
        //         Debug.LogError($"Actor asset not found at path: {path}");
        //         return null;
        //     }

        //     var skeletonDataAsset = GameObject.Instantiate(original) as SkeletonDataAsset;

        //     // var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
        //     Debug.Assert(skeletonDataAsset != null, $"SkeletonDataAsset not found at path: {path}");
        //     var newSkeleton = new GameObject(skeletonDataAsset.name);
        //     var skeletonAnimation = newSkeleton.AddComponent<SkeletonAnimation>();
        //     skeletonAnimation.skeletonDataAsset = skeletonDataAsset;
        //     skeletonAnimation.Initialize(true);

        //     // skeletonAnimation.skeleton.SetSkin(skins.Items[skinIndex]);
        //     skeletonAnimation.Skeleton.SetSlotsToSetupPose();

        //     return skeletonAnimation.gameObject;
        // }

        protected UnityEngine.Object GetActorAssetInfo(string path)
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
