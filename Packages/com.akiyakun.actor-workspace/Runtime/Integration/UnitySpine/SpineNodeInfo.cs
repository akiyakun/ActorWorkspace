#nullable enable
using UnityEngine;

namespace ActorWorkspace.UnitySpine
{
    // Spineのボーンやスロットを同時に扱いたかったので作成
    [System.Serializable]
    public class SpineNodeInfo
    {
        [SerializeField]
        public SpineNodeType NodeType = SpineNodeType.Bone;

        [SerializeField]
        public string Name = "";

        // Spine.Bone bone;
        // public Spine.Bone Bone => bone;
    }
}
#nullable restore