#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace.ActorBehaviour
{
    // 2Dの向き(左、右)の振る舞い
    //
    // Variables:
    // FacingDirection2Dを使用します。
    //
    // Events:
    // OnFacingDirection2DChangedを発行します。
    //
    public class AWFacingDirection2DBehaviour : AWActorBehaviour<IAWActor>
    {
        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Update;// | UpdateFlags.LateUpdate;

        Variable facingDirection2D = null!;
        FacingDirection2D prevFacingDirection2D;

        public override void OnRestore()
        {
            prevFacingDirection2D = GetFacingDirection();
            facingDirection2D.Int = (int)prevFacingDirection2D;
            Actor.EventBus.Publish(AWCoreActorEvents.OnFacingDirection2DChanged, (int)prevFacingDirection2D);
        }

        public override void OnAwake()
        {
            facingDirection2D = Variables.Get(AWCoreVariableKey.FacingDirection2D);
            if (facingDirection2D == null) throw new System.Exception();
        }

        public override void OnUpdate(float deltaTime)
        {
            var currentFacingDirection2D = GetFacingDirection();
            if (currentFacingDirection2D != prevFacingDirection2D)
            {
                facingDirection2D.Int = (int)currentFacingDirection2D;
                Actor.EventBus.Publish(AWCoreActorEvents.OnFacingDirection2DChanged, (int)currentFacingDirection2D);

                prevFacingDirection2D = currentFacingDirection2D;
            }
        }

        public FacingDirection2D GetFacingDirection()
        {
            if (Actor.ActorDisplay.IsScaleSign(ValueSignType.PositiveX))
            {
                return FacingDirection2D.Right;
            }
            else
            {
                return FacingDirection2D.Left;
            }
        }

    }
}
#nullable restore