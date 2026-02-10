#nullable enable
using Cysharp.Threading.Tasks;
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
        public virtual async UniTask OnNewImported(string path, SkeletonDataAsset skeletonDataAsset, SpineExtraDataScriptableObject spineExtraData)
        {
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 更新インポートされたときにコールバックされる
        /// </summary>
        public virtual async UniTask OnUpdateImported(string path, SkeletonDataAsset skeletonDataAsset, SpineExtraDataScriptableObject spineExtraData)
        {
            await UniTask.CompletedTask;
        }
    }
}
#nullable restore