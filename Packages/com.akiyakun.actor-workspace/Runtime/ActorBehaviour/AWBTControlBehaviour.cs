#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace.ActorBehaviour
{
    // BehaviorTreeのコントロール用
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
            behaviorTree.enabled = true;
        }

        protected virtual MonoBehaviour GetBehaviorTree()
        {
            if (Actor.Variables.TryGet(BehaviorTreeVariableName, out var value) == false)
            {
                throw new System.Exception($"BehaviorTree variable not found. ActorId={Actor.ActorId}");
            }
            var monoBehaviour = value.GetComponent<MonoBehaviour>();
            if (monoBehaviour == null) throw new System.Exception($"BehaviorTree component not found. ActorId={Actor.ActorId}");
            return monoBehaviour;
        }

    }
}
#nullable restore