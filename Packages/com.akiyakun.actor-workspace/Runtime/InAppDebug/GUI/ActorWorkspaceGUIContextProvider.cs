using UnityEngine;
using afl;
using afl.MasterData;
using ActorWorkspace.MasterData;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceGUIContextProvider : UniversalContextProvider
    {
        public IAssetRepository AssetRepository { get; private set; } = new MockAssetRepository();
        public IAWActorFactory ActorFactory { get; private set; } = new MockAWActorFactory();

        public WorkingActorContext CurrentWorkingActorContext { get; set; }

        public ActorWorkspaceGUIContextProvider()
        {
            AssetRepository = new MockAssetRepository();
            ActorFactory = new MockAWActorFactory();
        }

        // public ActorWorkspaceGUIContextProvider(IAssetRepository assetRepository, IAWActorFactory actorFactory)
        // {
        //     var actorAssetCollectionInAssetDatabase = new ActorAssetCollectionInAssetDatabase();
        //     actorAssetCollectionInAssetDatabase.assetRootDirectory = "Assets/AssetBundleData/Actor";
        //     actorAssetCollectionInAssetDatabase.Init();

        //     AssetRepository = actorAssetCollectionInAssetDatabase;
        //     ActorFactory = new SpineActorFactoryInAssetDatabase(AssetRepository);
        // }
    }
}
