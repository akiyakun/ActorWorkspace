#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.UnitySpine
{
    // SpineMecanimAnimationController 初期化時アタッチされていなければエラーになります。
    [RequireComponent(typeof(SpineMecanimAnimationEventDetector))]
    public class SpineMecanim : MonoBehaviour
    {
        [SerializeField] SpineExtraDataScriptableObject spineExtraData = null!;
        public SpineExtraDataScriptableObject SpineExtraData => spineExtraData;
    }
}
#nullable restore