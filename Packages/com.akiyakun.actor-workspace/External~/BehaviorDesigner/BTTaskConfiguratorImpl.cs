#nullable enable
using UnityEngine;
using BehaviorDesigner.Runtime;
using afl.BehaviorTask;

namespace Project.BehaviorTask
{
    [CreateAssetMenu(menuName = "App/BehaviorTask/BTTaskConfiguratorImpl", fileName = "BTTaskConfiguratorImplSO")]
    public partial class BTTaskConfiguratorImpl : BTTaskConfigurator
    {
        public override IBTTaskController CreateTaskController(GameObject owner)
        {
            var controller = new TaskController(owner);
            return controller;
        }
    }
}
#nullable restore