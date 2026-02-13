#nullable enable
using UnityEngine;
using afl;
using afl.BehaviorTask;

namespace ActorWorkspace.ActorBehaviour
{
    // 外部BehaviorTree機能のコントロール用
    public class AWBTTaskControllerBridgeBehaviour : AWActorBehaviour<IAWActor>
    {
        public const string BTTaskControllerFactoryVariableName = "BTTaskControllerFactory";

        public override UpdateFlags UpdateFlags { get; set; } = UpdateFlags.Update;

        BTTaskControllerFactory? taskControllerFactory;
        IBTTaskController? taskController;

        public override void Restore()
        {
        }

        public override void DoAwake()
        {
            Debug.Log($"AWBTTaskControllerBridgeBehaviour DoAwake ActorId={Actor.ActorId}");
            taskControllerFactory = GetTaskControllerFactory();
            if (taskControllerFactory != null)
            {
                taskController = taskControllerFactory.CreateTaskController(Actor.GameObject);
                if (taskController == null) throw new System.Exception($"Failed to create BTTaskController. ActorId={Actor.ActorId}");
                taskController.DoAwake();
                // taskControllerFactory.enabled = true;
            }
        }

        public override void DoDestroy()
        {
        }

        public override void DoUpdate(float deltaTime)
        {
            taskController?.DoUpdate(deltaTime);
        }

        protected virtual BTTaskControllerFactory? GetTaskControllerFactory()
        {
            if (Actor.Variables.TryGet(BTTaskControllerFactoryVariableName, out var value) == false)
            {
                throw new System.Exception($"Variable not found. ActorId={Actor.ActorId}");
            }
            // BTTaskControllerFactory aa;
            // aa.g
            taskControllerFactory = value.GetUnityObject<BTTaskControllerFactory>();
            if (taskControllerFactory == null)
            {
                // throw new System.Exception($"BehaviorTree component not found. ActorId={Actor.ActorId}");
                return null;
            }
            return taskControllerFactory;
        }

    }
}
#nullable restore