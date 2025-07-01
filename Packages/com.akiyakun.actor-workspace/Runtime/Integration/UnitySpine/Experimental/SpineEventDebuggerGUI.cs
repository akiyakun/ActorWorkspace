using UnityEngine;
using Spine;
using Spine.Unity;
using System.Collections.Generic;

namespace ActorWorkspace.InAppDebug
{
    public class SpineEventDebuggerGUI : MonoBehaviour
    {
        public SkeletonAnimation skeletonAnimation;
        private Queue<string> eventLogs = new Queue<string>();
        public int maxLogs = 10;

        public void Set()
        {
            skeletonAnimation.AnimationState.Event += OnSpineEvent;
        }

        void OnSpineEvent(TrackEntry entry, Spine.Event e)
        {
            string msg = $"[{Time.time:F2}s] Event: {e.Data.Name} @ {entry.Animation.Name} time={entry.AnimationTime:F2}s";
            eventLogs.Enqueue(msg);

            // 最大ログ数制限
            while (eventLogs.Count > maxLogs)
                eventLogs.Dequeue();
        }

        void OnGUI()
        {
            if (skeletonAnimation == null) return;

            GUILayout.BeginArea(new Rect(10, 10 + 600, 600, 300));
            GUILayout.Label("<b><size=15>Spine Events</size></b>");
            foreach (string log in eventLogs)
            {
                GUILayout.Label(log);
            }
            GUILayout.EndArea();
        }
    }
}
