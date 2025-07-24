#nullable enable
using UnityEngine;
using afl;
using afl.MasterData;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceFormContextProvider : UniversalContextProvider
    {
        public AssetRepositoryCategorize AssetRepositories { get; private set; }
        public IAWActorFactory ActorFactory { get; private set; }

        public WorkingActorContext? CurrentWorkingActorContext { get; set; }

        public ActorWorkspaceFormContextProvider()
        {
            AssetRepositories = new();
            AssetRepositories.AddCategory(0, new afl.MasterData.Tests.DummyAssetRepository());

            ActorFactory = new Tests.FakeAWActorFactory();
        }

        public ActorWorkspaceFormContextProvider(AssetRepositoryCategorize assetRepositories, IAWActorFactory actorFactory)
        {
            AssetRepositories = assetRepositories;
            ActorFactory = actorFactory;
        }
    }
}
#nullable restore