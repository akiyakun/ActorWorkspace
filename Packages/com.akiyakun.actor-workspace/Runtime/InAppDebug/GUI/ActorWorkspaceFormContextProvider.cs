#nullable enable
using UnityEngine;
using afl;
using afl.MasterData;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceFormContextProvider : UniversalContextProvider
    {
        public AssetRepositoryCategorize AssetRepositories { get; private set; }

        public IAWActorManager ActorManager { get; private set; }
        public AWActorContextProvider ActorContextProvider { get; private set; }
        public IAWActorFactory ActorFactory { get; private set; }

        public WorkingActorContext? CurrentWorkingActorContext { get; set; }

        public ActorWorkspaceFormContextProvider()
        {
            AssetRepositories = new();
            AssetRepositories.AddCategory(0, new afl.MasterData.Tests.DummyAssetRepository());

            ActorManager = new AWActorManager();
            ActorContextProvider = new AWActorContextProvider(ActorManager);
            ActorFactory = new Tests.FakeAWActorFactory(ActorContextProvider);
        }

        public ActorWorkspaceFormContextProvider(AssetRepositoryCategorize assetRepositories,
            IAWActorManager actorManager, AWActorContextProvider actorContextProvider, IAWActorFactory actorFactory)
        {
            AssetRepositories = assetRepositories;
            Debug.Assert(assetRepositories != null);

            ActorManager = actorManager;
            Debug.Assert(actorManager != null);

            ActorContextProvider = actorContextProvider;
            Debug.Assert(actorContextProvider != null);

            ActorFactory = actorFactory;
            Debug.Assert(actorFactory != null);

        }

        public override void Release()
        {
        }
    }
}
#nullable restore