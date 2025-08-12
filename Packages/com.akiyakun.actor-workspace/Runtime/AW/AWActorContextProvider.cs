#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;
using afl.MasterData;

namespace ActorWorkspace
{
    public class AWActorContextProvider : UniversalContextProvider
    {
        public virtual AssetRepositoryCategorize AssetRepositoryCategorize { get; protected set; }
        // public virtual IAWActorManager ActorManager => awActorManager;

// #nullable disable
//         private AWActorContextProvider() { }
// #nullable enable

        // IAWActorManager awActorManager;

        // public AWActorContextProvider(IAWActorManager awActorManager)
        public AWActorContextProvider(
            AssetRepositoryCategorize assetRepositoryCategorize
        )
        {
            AssetRepositoryCategorize = assetRepositoryCategorize;
            Debug.Assert(AssetRepositoryCategorize != null);

            // this.awActorManager = awActorManager;
            // Debug.Assert(awActorManager != null);
        }

        public override void Release()
        {
        }
    }
}
#nullable restore