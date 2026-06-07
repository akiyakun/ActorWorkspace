#nullable enable
using UnityEngine;
using afl;
using afl.BehaviorTask;

namespace ActorWorkspace.ActorBehaviour
{
    // 外部BehaviorTree機能のコントロール用
    public class AWBTTaskControllerBridgeBehaviour : AWActorBehaviour<IAWActor>
    {
        public const string BTTaskConfiguratorVariableName = "BTTaskConfigurator";

        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Update;

        BTTaskConfigurator? taskConfigurator;
        IBTTaskController? taskController;

        public override void OnRestore()
        {
        }

        public override void OnAwake()
        {
            // Debug.Log($"AWBTTaskControllerBridgeBehaviour DoAwake ActorId={Actor.ActorId}");
            taskConfigurator = GetTaskConfigurator();
            if (taskConfigurator != null)
            {
                taskController = taskConfigurator.CreateTaskController(Actor.ActorDisplay.Main);
                if (taskController == null) throw new System.Exception($"Failed to create BTTaskController. ActorId={Actor.ActorId}");
                taskController.DoAwake();
                // taskControllerFactory.enabled = true;
            }
        }

        public override void OnDestroy()
        {
        }

        public override void OnUpdate(float deltaTime)
        {
            taskController?.DoUpdate(deltaTime);
        }

        protected virtual BTTaskConfigurator? GetTaskConfigurator()
        {
            if (Actor.Variables.TryGet(BTTaskConfiguratorVariableName, out var value) == false)
            {
                throw new System.Exception($"Variable not found. ActorId={Actor.ActorId}");
            }
            taskConfigurator = value.GetUnityObject<BTTaskConfigurator>();
            if (taskConfigurator == null)
            {
                // throw new System.Exception($"BehaviorTree component not found. ActorId={Actor.ActorId}");
                return null;
            }
            return taskConfigurator;
        }

        public void SetupParameters(System.Action<IBTTaskController> setupAction)
        {
            taskController?.SetupParameters(setupAction);
        }


        #region Event
        public void SendEvent(string name)
        {
            taskController?.SendEvent(name);
        }

        public void SendEvent<T>(string name, T arg1)
        {
            taskController?.SendEvent(name, arg1);
        }

        public void SendEvent<T, U>(string name, T arg1, U arg2)
        {
            taskController?.SendEvent(name, arg1, arg2);
        }

        public void SendEvent<T, U, V>(string name, T arg1, U arg2, V arg3)
        {
            taskController?.SendEvent(name, arg1, arg2, arg3);
        }
        #endregion
    }
}
#nullable restore