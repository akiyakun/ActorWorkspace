#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;
using afl;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    // [CreateAssetMenu(menuName = "App/ProjectSpineImport", fileName = "ProjectSpineImport")]
    // [System.Serializable]
    public class UnitySpineImportCallback : ScriptableObjectCustom
    {
        /// <summary>
        /// 新規インポートされたときにコールバックされる
        /// コールバック後に SaveAssets() されます。
        /// </summary>
        public virtual async UniTask OnNewImported(string path, SkeletonDataAsset skeletonDataAsset, SpineExtraDataScriptableObject spineExtraData)
        {
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 更新インポートされたときにコールバックされる
        /// コールバック後に SaveAssets() されます。
        /// </summary>
        public virtual async UniTask OnUpdateImported(string path, SkeletonDataAsset skeletonDataAsset, SpineExtraDataScriptableObject spineExtraData)
        {
            await UniTask.CompletedTask;
        }
    }
}
#nullable restore