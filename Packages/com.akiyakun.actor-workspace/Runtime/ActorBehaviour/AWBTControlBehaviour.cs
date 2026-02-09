#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace.ActorBehaviour
{
    // 外部BehaviorTree機能のコントロール用
    public class AWBTControlBehaviour : AWActorBehaviour<IAWActor>
    {
        public const string BehaviorTreeVariableName = "BehaviorTree";

        public override UpdateFlags UpdateFlags { get; set; } = UpdateFlags.Manual;

        MonoBehaviour? behaviorTree;

        public override void Restore()
        {
        }

        public override void DoAwake()
        {
            Debug.Log($"AWBTControlBehaviour DoAwake ActorId={Actor.ActorId}");
            behaviorTree = GetBehaviorTree();
            if (behaviorTree != null)
            {
                behaviorTree.enabled = true;
            }
        }

        protected virtual MonoBehaviour? GetBehaviorTree()
        {
            if (Actor.Variables.TryGet(BehaviorTreeVariableName, out var value) == false)
            {
                throw new System.Exception($"BehaviorTree variable not found. ActorId={Actor.ActorId}");
            }
            var monoBehaviour = value.GetComponent<MonoBehaviour>();
            if (monoBehaviour == null)
            {
                // throw new System.Exception($"BehaviorTree component not found. ActorId={Actor.ActorId}");
                return null;
            }
            return monoBehaviour;
        }

    }
}
#nullable restore