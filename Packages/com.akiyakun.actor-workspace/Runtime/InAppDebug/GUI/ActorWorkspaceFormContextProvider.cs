#nullable enable
using System.Collections.Generic;
using UnityEngine;
using ActorWorkspace.MasterData;
using afl.UI;
using afl.MasterData;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceFormContextProvider : UIContextProvider
    {
        public AssetRepositoryCategorize AssetRepositories { get; private set; }

        // public AWActorContextProvider ActorContextProvider { get; private set; }
        public IAWActorFactory ActorFactory { get; private set; }
        public IAWActorManager ActorManager { get; private set; }

        public WorkingActorContext CurrentWorkingActorContext { get; set; } = new();

        // カテゴリーの表示名。足りない分は番号で表す
        public IReadOnlyList<string> CategoryNames { get; set; } = System.Array.Empty<string>();

        // 一覧表示で再生するモーション。無いアクターはモーションの一覧の先頭を再生する
        public string CatalogMotionName { get; set; } = string.Empty;

        public ActorWorkspaceFormContextProvider()
        {
            AssetRepositories = new();
            AssetRepositories.AddCategory(0, new afl.MasterData.Tests.StubAssetRepository());

            // ActorContextProvider = new AWActorContextProvider(AssetRepositories);
            ActorFactory = new Tests.StubAWActorFactory();
            ActorManager = new AWActorManager<IAWActor>(ActorFactory);
        }

        public ActorWorkspaceFormContextProvider(AssetRepositoryCategorize assetRepositories,
            IAWActorFactory actorFactory, IAWActorManager actorManager)
        {
            AssetRepositories = assetRepositories;
            Debug.Assert(assetRepositories != null);

            // ActorContextProvider = actorContextProvider;
            // Debug.Assert(actorContextProvider != null);

            ActorFactory = actorFactory;
            Debug.Assert(actorFactory != null);

            ActorManager = actorManager;
            Debug.Assert(actorManager != null);

        }

        public string GetCategoryName(int category)
        {
            return category < CategoryNames.Count ? CategoryNames[category] : category.ToString();
        }

        // カテゴリーのアセットのリストを生成して返します
        public List<ActorAssetInfo> GetAssetList(int category)
        {
            List<ActorAssetInfo> list = new();

            var assetRepository = AssetRepositories.Get(category);
            Debug.Assert(assetRepository != null);

            IReadOnlyList<IModel> database = assetRepository!.ToList();
            for (int i = 0; i < database.Count; ++i)
            {
                var model = database[i] as AssetModel;
                Debug.Assert(model != null);

                var info = new ActorAssetInfo();
                info.AssetModel = model!;
                info.Category = category;

                info.Name = model!.GetName();
                // Debug.Log($"AssetModel: {info.Name}");
                if (string.IsNullOrEmpty(info.Name))
                {
                    // info.Name = System.IO.Path.GetDirectoryName(info.AssetModel.AssetLocator).Replace(assetRootDirectory, "");
                    // info.Name = System.IO.Path.GetFileNameWithoutExtension(info.AssetModel.AssetLocator);
                    info.Name = $"{model.Id:0000}: {info.AssetModel.AssetLocator}";
                }

                list.Add(info);
            }

            return list;
        }
    }
}
#nullable restore