#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.UnitySpine
{
    // SkeletonMecanim 用
    // AnimationClipからイベントを取得するにはMonoBehaviourが必要なため。
    public class SpineMecanimAnimationEventDetector : MonoBehaviour
    {
        SpineMecanimAnimationController controller = null!;

        // Call from SpineMecanimAnimationController
        public bool Initialize(SpineMecanimAnimationController spineMecanimAnimationController)
        {
            if (spineMecanimAnimationController == null) return false;
            controller = spineMecanimAnimationController;
            return true;
        }

        public void OnSpineEvent(AnimationEvent animationEvent)
        {
#if UNITY_EDITOR
            if (controller == null) throw new System.Exception("SpineMecanimAnimationEventDetector is Initialize() must be called.");
#endif

            // Debug.Log($"OnSpineEvent: name={animationEvent.functionName} intParameter={animationEvent.intParameter}, floatParameter={animationEvent.floatParameter}, stringParameter={animationEvent.stringParameter}");
            controller.OnSpineEvent(animationEvent);
        }

    }
}
#nullable restore