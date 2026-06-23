#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace.ActorBehaviour
{
    public abstract class AWTargetSelectorBehaviour : AWActorBehaviour<IAWActor>
    {
        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Update;// | UpdateFlags.LateUpdate;

        ITargetSelector targetSelector = null!;
        // public ITargetSelector TargetSelector => battleTargetSelector;

        // TargetSelectingData? targetSelectingData;

        // public override void Restore()
        // {
        // }

        public override void Awake()
        {
            base.Awake();

            targetSelector = BuildTargetSelector();
            if (targetSelector == null) throw new System.Exception();

            // Variableの設定
            {
                var variable = Actor.Variables.Get(AWCoreVariableKeys.TargetSelector);
                if (variable == null) throw new System.Exception();
                variable.SetNativeObject(targetSelector);
            }
        }

        protected abstract ITargetSelector BuildTargetSelector();

        // public override void OnUpdate(float deltaTime)
        // {
        // }
    }
}
#nullable restore