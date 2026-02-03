#nullable enable
using UnityEngine;
using afl;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    // [System.Serializable]
    public class UnitySpineImportCallback : ScriptableObjectCustom
    {
        /// <summary>
        /// 新規インポートされたときにコールバックされる
        /// </summary>
        public virtual void OnNewImported(string path, SkeletonDataAsset skeletonDataAsset, SpineExtraDataScriptableObject spineExtraData)
        {
        }
    }
}
#nullable restore