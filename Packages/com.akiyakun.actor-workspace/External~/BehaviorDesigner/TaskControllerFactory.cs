#nullable enable
using UnityEngine;
using BehaviorDesigner.Runtime;
using afl.BehaviorTask;

namespace Project.BehaviorTask
{
    [CreateAssetMenu(menuName = "App/BehaviorTask/TaskControllerFactory", fileName = "TaskControllerFactorySO")]
    public class TaskControllerFactory : BTTaskControllerFactory
    {
        public override IBTTaskController CreateTaskController(GameObject owner)
        {
            var controller = new TaskController(owner);
            return controller;
        }
    }
}
#nullable restore