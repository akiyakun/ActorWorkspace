#nullable enable
using UnityEngine;
using BehaviorDesigner.Runtime;
using afl.BehaviorTask;

namespace Project.BehaviorTask
{
    public class TaskController : IBTTaskController
    {
        public bool Enabled { get; protected set; }

        GameObject owner;
        BehaviorTree behaviorTree = null!;

        // owner: Actor.ActorDisplay.Main
        public TaskController(GameObject owner)
        {
            this.owner = owner;

            behaviorTree = owner.GetComponent<BehaviorTree>();
            // Debug.Assert(behaviorTree != null);
            Enabled = behaviorTree != null;
        }

        public void DoAwake()
        {
            if (Enabled == false) return;

            // behaviorTree.EnableBehavior();
            behaviorTree.StartWhenEnabled = true;
            behaviorTree.enabled = true;

            //     var factory = (IBTTaskFactory)GlobalVariables.Instance.GetVariable("BTTaskFactory").GetValue();
            //     Debug.Assert(factory != null);
            //     btConditional = factory.CreateConditional(Id, this);
            //     if (btConditional == null) throw new System.Exception();
            //     OnSetup();
            //     btConditional.OnAwake();
        }

        public void DoUpdate(float deltaTime)
        {
            if (Enabled == false) return;

            // FIXME: deltaTimeを渡したい
            // FIXME: FixedUpdateしたい場合どうしたら？
            //BehaviorDesigner.Runtime.Behavior
            // マニュアル更新
            BehaviorManager.instance.Tick(behaviorTree);
        }

        public void SetupParameters(System.Action<IBTTaskController> setupAction)
        {
            if (setupAction == null) return;
            setupAction.Invoke(this);
        }

        public void SetParameter<T>(string key, T value)
        {
            SharedVariable sharedVariable = value switch
            {
                int intValue => new SharedInt { Value = intValue },
                float floatValue => new SharedFloat { Value = floatValue },
                bool boolValue => new SharedBool { Value = boolValue },
                string stringValue => new SharedString { Value = stringValue },
                Vector3 vector3Value => new SharedVector3 { Value = vector3Value },
                GameObject gameObjectValue => new SharedGameObject { Value = gameObjectValue },
                _ => throw new System.Exception($"Unsupported parameter type. Type={typeof(T)}"),
            };

            behaviorTree.SetVariable(key, sharedVariable);
        }
    }
}
#nullable restore