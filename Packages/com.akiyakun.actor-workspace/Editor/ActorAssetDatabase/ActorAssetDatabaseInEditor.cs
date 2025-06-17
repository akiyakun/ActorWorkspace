#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using afl;

using UnityEditor;
using afl.Editor;

namespace ActorWorkspace.ActorAssetDatabase.Editor
{
    public abstract class ActorAssetDatabaseInEditor : MonoBehaviour, IActorAssetDatabase
    {
        protected List<ActorAssetInfo> actorAssetInfoList = new();

        protected virtual void Awake()
        {
        }

        public List<ActorAssetInfo> GetAll()
        {
            return actorAssetInfoList;
        }

        public abstract GameObject CreateActorAsset(string path);
    }
}
#endif
