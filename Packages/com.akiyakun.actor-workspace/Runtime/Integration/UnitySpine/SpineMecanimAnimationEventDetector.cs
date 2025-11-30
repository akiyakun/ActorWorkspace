#nullable enable
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
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

        // public void OnEvent()
        // {
        //     Debug.Log($"OnEvent 0 : ");
        // }

        // public void OnEvent(int value)
        // {
        //     Debug.Log($"OnEvent : int={value}");
        // }

        // public void OnEvent(float value)
        // {
        //     Debug.Log($"OnEvent : float={value}");
        // }

        // public void OnEvent(string value)
        // {
        //     Debug.Log($"OnEvent : string={value}");
        // }

        public void OnSpineEvent(AnimationEvent animationEvent)
        {
#if UNITY_EDITOR
            if (controller == null) throw new System.Exception("SpineMecanimAnimationEventDetector is Initialize() must be called.");
#endif

            Debug.Log($"OnSpineEvent: name={animationEvent.functionName} intParameter={animationEvent.intParameter}, floatParameter={animationEvent.floatParameter}, stringParameter={animationEvent.stringParameter}");
            controller.OnSpineEvent(animationEvent);
        }


        // public void NotifySpineEvent(string eventName, float eventTime, int intValue, float floatValue, string stringValue)
        // {
        //     // イベント名と時間を受け取って、必要な処理を行う
        //     Debug.Log($"eventName={eventName}, eventTime={eventTime}, intValue={intValue}, floatValue={floatValue}, stringValue={stringValue}");
        //     controller.OnSpineEvent(eventName, eventTime, intValue, floatValue, stringValue);
        // }

    }
}
#nullable restore