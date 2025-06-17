using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.ActorAssetDatabase
{
    public interface IActorAssetDatabase
    {
        List<ActorAssetInfo> GetAll();

        GameObject CreateActorAsset(string path);
    }
}
