using UnityEngine;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.InAppDebug
{
    [ExecuteAlways]
    public class SpineEventGizmoVisualizer : MonoBehaviour
    {
        public SkeletonAnimation skeletonAnimation;
        public string targetEventName = ""; // 空なら全イベント表示

        void OnDrawGizmos()
        {
            if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
                return;

            var skeleton = skeletonAnimation.Skeleton;
            var state = skeletonAnimation.AnimationState;
            var current = state.GetCurrent(0);
            if (current == null)
                return;

            // イベント時間を取得
            var timeline = current.Animation.Timelines;
            foreach (var t in timeline)
            {
                if (t is EventTimeline evtTimeline)
                {
                    for (int i = 0; i < evtTimeline.FrameCount; i++)
                    {
                        float time = evtTimeline.Frames[i];
                        EventData ed = evtTimeline.Events[i].Data;

                        if (string.IsNullOrEmpty(targetEventName) || ed.Name == targetEventName)
                        {
                            Bone bone = skeletonAnimation.Skeleton.FindBone("katana_effects");
                            // bone.UpdateWorldTransform();
                            Vector3 worldPos = skeletonAnimation.transform.TransformPoint(new Vector3(bone.WorldX, bone.WorldY, 0));


                            Vector3 pos = worldPos;
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawWireSphere(pos, 0.15f);
                            // #if UNITY_EDITOR
                            UnityEditor.Handles.color = Color.blue;
                            UnityEditor.Handles.Label(pos + (Vector3.up * 0.1f), $"{ed.Name} @ {time:F2}s");
// #endif
                        }
                    }
                }
            }
        }
    }
}
