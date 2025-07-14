using UnityEngine;
using afl;
using afl.MasterData;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceGUIContextProvider : UniversalContextProvider
    {
        public AssetRepositoryCategorize AssetRepositories { get; private set; }
        public IAWActorFactory ActorFactory { get; private set; } = new MockAWActorFactory();

        public WorkingActorContext CurrentWorkingActorContext { get; set; }

        public ActorWorkspaceGUIContextProvider()
        {
            AssetRepositories = new();
            AssetRepositories.AddCategory(0, new MockAssetRepository());

            ActorFactory = new MockAWActorFactory();
        }

        public ActorWorkspaceGUIContextProvider(AssetRepositoryCategorize assetRepositories, IAWActorFactory actorFactory)
        {
            AssetRepositories = assetRepositories;
            ActorFactory = actorFactory;
        }
    }
}
