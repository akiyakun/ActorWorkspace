#nullable enable
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    public class SpineMecanimAnimationEventDetector : MonoBehaviour
    {
        // 例：
        // void Event_01()
        // {
        //      NotifySpineEvent(nameof(Event_01), Time.time);
        // }
        //
        public void NotifySpineEvent(string eventName, float eventTime, int intValue, float floatValue, string stringValue)
        {
            // イベント名と時間を受け取って、必要な処理を行う
            // ここでは、イベント名と時間をログに出力する例を示します
            // Debug.Log($"eventName={eventName}, eventTime={eventTime}, intValue={intValue}, floatValue={floatValue}, stringValue={stringValue}");
            controller?.OnSpineEvent(eventName, eventTime, intValue, floatValue, stringValue);
        }

        SpineMecanimAnimationController? controller;
        public void Setup(SpineMecanimAnimationController spineMecanimAnimationController)
        {
            controller = spineMecanimAnimationController;
        }
    }
}
#nullable restore