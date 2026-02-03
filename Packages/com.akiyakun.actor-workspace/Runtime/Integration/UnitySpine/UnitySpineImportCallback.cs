#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace.UnitySpine
{
    // [System.Serializable]
    public class UnitySpineImportCallback : ScriptableObjectCustom
    {
        /// <summary>
        /// 新規インポートされたときにコールバックされる
        /// </summary>
        public virtual void OnNewImported(string path)
        {
        }
    }
}
#nullable restore