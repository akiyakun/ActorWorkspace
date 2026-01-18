#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.UnitySpine
{
    // SpineSkeletonAnimationController 初期化時アタッチされていなければエラーになります。
    public class SpineSkeleton : MonoBehaviour
    {
        [SerializeField] SpineExtraDataScriptableObject spineExtraData = null!;
        public SpineExtraDataScriptableObject SpineExtraData => spineExtraData;
    }
}
#nullable restore