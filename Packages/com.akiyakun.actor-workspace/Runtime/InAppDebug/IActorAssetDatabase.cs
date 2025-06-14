using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.InAppDebug
{
    // デバッグ用の情報取得用インターフェース
    public interface IActorAssetDatabase
    {
        List<ActorAssetInfo> GetAll();

        GameObject CreateActorAsset(string path);
    }
}
