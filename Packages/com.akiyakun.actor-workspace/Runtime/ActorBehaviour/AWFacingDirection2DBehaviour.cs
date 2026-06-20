#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace.ActorBehaviour
{
    // 2Dの向き(左、右)の振る舞い
    //
    // Variables:
    // FacingDirection2D を使用します。
    //
    // Events:
    // OnFacingDirection2DChanged を発行します。
    //
    public class AWFacingDirection2DBehaviour : AWActorBehaviour<IAWActor>
    {
        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Update;// | UpdateFlags.LateUpdate;

        Variable facingDirection2D = null!;
        FacingDirection2D prevFacingDirection2D;

        public override void OnAwake()
        {
            if (Variables.TryGet(AWCoreVariableKeys.FacingDirection2D, out facingDirection2D) == false)
            {
                throw new System.Exception($"AWFacingDirection2DBehaviour: Missing variable {AWCoreVariableKeys.FacingDirection2D}");
            }
        }

        public override void OnRestore()
        {
            prevFacingDirection2D = GetFacingDirection();
            facingDirection2D.Int = (int)prevFacingDirection2D;
            Actor.EventBus.Publish(AWCoreActorEvents.OnFacingDirection2DChanged, (int)prevFacingDirection2D);
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