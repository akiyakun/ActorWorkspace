#nullable enable
using UnityEngine;
using BehaviorDesigner.Runtime;
using afl.BehaviorTask;

namespace Project.BehaviorTask
{
    public class TaskController : IBTTaskController
    {
        GameObject owner;
        BehaviorTree behaviorTree = null!;

        public TaskController(GameObject owner)
        {
            this.owner = owner;

            behaviorTree = owner.GetComponent<BehaviorTree>();
            Debug.Assert(behaviorTree != null);
        }

        public void DoAwake()
        {
            // behaviorTree.EnableBehavior();
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
            // FIXME: deltaTimeを渡したい
            // FIXME: FixedUpdateしたい場合どうしたら？
            //BehaviorDesigner.Runtime.Behavior
            // マニュアル更新
            BehaviorManager.instance.Tick(behaviorTree);
        }

    }
}
#nullable restore