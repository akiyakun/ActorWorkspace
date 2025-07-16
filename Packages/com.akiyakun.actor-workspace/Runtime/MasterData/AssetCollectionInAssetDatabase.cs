#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using afl;
using afl.MasterData;

using UnityEditor;
// using afl.Editor;

namespace ActorWorkspace.MasterData
{
    /// <summary>
    /// AssetDatabaseからアセットリストを作成
    /// Editor専用
    /// </summary>
    public class AssetCollectionInAssetDatabase<TAsset> : IAssetRepository
         where TAsset : UnityEngine.Object
    {
        [SerializeField] public string assetRootDirectory;
        // [SerializeReference] public System.Type actorAssetType;
        // [SerializeReference] public IAWActorFactory actorFactory;

        protected List<AssetModel> assetList = new();


        // From IRawMasterData
        public virtual int Length => assetList.Count;

        // From IRawMasterData
        public virtual IModel this[int index] => assetList[index];


        // From IRepository
        public T Get<T>(int id) where T : class, IModel
        {
            int count = assetList.Count;
            for (int i = 0; i < count; ++i)
            {
                var info = assetList[i];
                if (info.Id == id)
                {
                    return info as T;
                }
            }

            return null;
        }

        // From IRepository
        public IReadOnlyList<IModel> ToList()
        {
            return assetList.ToList<IModel>();
        }

        // From IAssetRepository
        public AssetModel GetAssetModel(int id) => Get<AssetModel>(id);


        // protected virtual void Awake()
        public void Init()
        {
            Debug.Assert(string.IsNullOrEmpty(assetRootDirectory) == false);
            var targetFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(assetRootDirectory);
            Debug.Assert(targetFolder != null);
            IReadOnlyList<TAsset> result = FindAssets<TAsset>(targetFolder);
            for (int i = 0; i < result.Count; i++)
            {
                var asset = result[i];

                var model = new AssetModel();
                model.Id = i + 1; // MasterDataのIdは1から始まる
                model.AssetLocator = AssetDatabase.GetAssetPath(asset);
                // info.Name = Path.GetDirectoryName(info.AssetLocator).Replace(assetRootDirectory, "");
                // Debug.Log($"Found SkeletonDataAsset: {info.Name} at {info.Path}");
                assetList.Add(model);
            }
        }

        // // From IActorAssetCollection
        // public virtual List<ActorAssetInfo> GetAll()
        // {
        //     return assetList;
        // }

        // From IActorAssetCollection
        // public virtual async UniTask<IAWActor> CreateAsync(int index)
        // {
        //     return await actorFactory.CreateAsync(index);
        // }

        protected IReadOnlyList<T> FindAssets<T>(DefaultAsset folder) where T : UnityEngine.Object
        {
            return FindAssets<T>(new List<DefaultAsset> { folder });
        }

        static IReadOnlyList<T> FindAssets<T>(IReadOnlyList<DefaultAsset> folders) where T : UnityEngine.Object
        {
            if (folders == null)
            {
                return new List<T>();
            }

            var folderPathList = folders
                .Where(folder => folder != null)
                .Select(AssetDatabase.GetAssetPath)
                .Where(path => !string.IsNullOrEmpty(path))
                .Where(path => Directory.Exists(path))
                .ToList();

            if (folderPathList.Count <= 0)
            {
                return new List<T>();
            }

            var assets = AssetDatabase
                .FindAssets($"t:{typeof(T).Name}", searchInFolders: folderPathList.ToArray())
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path)
                .Distinct()
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .ToList();

            return assets;
        }
    }
}
#endif
