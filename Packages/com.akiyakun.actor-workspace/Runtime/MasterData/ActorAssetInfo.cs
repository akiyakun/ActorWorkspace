using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using afl.MasterData;

namespace ActorWorkspace.MasterData
{
    [System.Serializable]
    public class ActorAssetInfo : AssetModel
    {
        public string Name;
        // public string Path;

        // From IModel
        public override string GetName() => Name;
    }
}
