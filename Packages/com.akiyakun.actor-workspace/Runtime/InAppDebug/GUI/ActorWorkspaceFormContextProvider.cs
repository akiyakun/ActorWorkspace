#nullable enable
using UnityEngine;
using afl;
using afl.MasterData;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceFormContextProvider : UniversalContextProvider
    {
        public AssetRepositoryCategorize AssetRepositories { get; private set; }

        public AWActorContextProvider ActorContextProvider { get; private set; }
        public IAWActorFactory ActorFactory { get; private set; }
        public IAWActorManager ActorManager { get; private set; }

        public WorkingActorContext CurrentWorkingActorContext { get; set; } = new();

        public ActorWorkspaceFormContextProvider()
        {
            AssetRepositories = new();
            AssetRepositories.AddCategory(0, new afl.MasterData.Tests.DummyAssetRepository());

            ActorContextProvider = new AWActorContextProvider(AssetRepositories);
            ActorFactory = new Tests.FakeAWActorFactory(ActorContextProvider);
            ActorManager = new AWActorManager(ActorFactory);
        }

        public ActorWorkspaceFormContextProvider(AssetRepositoryCategorize assetRepositories,
            AWActorContextProvider actorContextProvider, IAWActorFactory actorFactory, IAWActorManager actorManager)
        {
            AssetRepositories = assetRepositories;
            Debug.Assert(assetRepositories != null);

            ActorContextProvider = actorContextProvider;
            Debug.Assert(actorContextProvider != null);

            ActorFactory = actorFactory;
            Debug.Assert(actorFactory != null);

            ActorManager = actorManager;
            Debug.Assert(actorManager != null);

        }

    }
}
#nullable restore