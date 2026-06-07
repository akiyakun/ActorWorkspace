#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace.ActorBehaviour
{
    // 基本的な戦闘に関するもの
    public abstract class AWBattleTargetBehaviour : AWActorBehaviour<IAWActor>
    {
        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Update;// | UpdateFlags.LateUpdate;

        IBattleTargetSelector battleTargetSelector = null!;
        public IBattleTargetSelector BattleTargetSelector => battleTargetSelector;

        // public override void Restore()
        // {
        // }

        public override void DoAwake()
        {
            battleTargetSelector = CreateTargetSelector();
            if (battleTargetSelector == null) throw new System.Exception();
        }

        protected abstract IBattleTargetSelector CreateTargetSelector();

    }
}
#nullable restore