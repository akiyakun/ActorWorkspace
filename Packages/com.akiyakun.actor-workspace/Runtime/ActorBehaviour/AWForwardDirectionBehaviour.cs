#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace.ActorBehaviour
{
    // アクターの前方方向の振る舞い
    //
    // Variables:
    // ForwardDirection を使用します。
    //
    public class AWForwardDirectionBehaviour : AWActorBehaviour<IAWActor>
    {
        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Update;// | UpdateFlags.LateUpdate;

        Variable forwardDirection = null!;

        public override void OnAwake()
        {
            forwardDirection = Variables.Get(AWCoreVariableKeys.ForwardDirection);
            if (forwardDirection == null) throw new System.Exception();
        }

        public override void OnRestore()
        {
            forwardDirection.SetVector3(Actor.ActorParam.ForwardDirection);
        }

        public override void OnUpdate(float deltaTime)
        {
            forwardDirection.SetVector3(Actor.ActorParam.ForwardDirection);
        }
    }
}
#nullable restore