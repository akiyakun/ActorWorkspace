using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Spine;
using Spine.Unity;


namespace ActorWorkspace.UnitySpine
{
    // public class SpineActorFactory : IAWActorFactory
    // {
    //     IAssetRepository assetRepository;

    //     private SpineActorFactory()
    //     {
    //     }

    //     public SpineActorFactory(IAssetRepository assetRepository)
    //     {
    //         Debug.Assert(assetRepository != null);
    //         this.assetRepository = assetRepository;
    //     }

    //     // From IActorFactory
    //     public virtual async UniTask<IAWActor> CreateAsync(int id)
    //     {
    //         // Debug.Assert((uint)id < (uint)masterData.Length);
    //         string assetName = assetRepository.GetAssetModel(id).AssetName;
    //         // return SpineUtility.CreateSkeletonAnimationFromAssetDatabae(assetName).gameObject;
    //     }

    // }
}
