#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace.ActorBehaviour
{
    public abstract class AWTargetSelectorBehaviour : AWActorBehaviour<IAWActor>
    {
        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Update;// | UpdateFlags.LateUpdate;

        ITargetSelector battleTargetSelector = null!;
        public ITargetSelector TargetSelector => battleTargetSelector;

        TargetSelectingData? targetSelectingData;

        // public override void Restore()
        // {
        // }

        public override void OnAwake()
        {
            battleTargetSelector = CreateTargetSelector();
            if (battleTargetSelector == null) throw new System.Exception();

            // TargetSelectingData を取得
            // TargetSelectingData からターゲット洗濯方法をしとく
        }

        protected abstract ITargetSelector CreateTargetSelector();

        public override void OnUpdate(float deltaTime)
        {

        }

        // protected GameObject? FindTarget()
        // {

        // }

    }
}
#nullable restore