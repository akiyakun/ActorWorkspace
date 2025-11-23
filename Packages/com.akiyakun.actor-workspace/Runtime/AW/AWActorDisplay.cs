#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // アクターの表示のルート
    // 全体をスケールしたり反転したりするよう
    // MonoBehaviour前提
    public class AWActorDisplay : MonoBehaviour
    {
        public bool ForceUnitScale { get; set; } = true;

        public bool IsVisibility
        {
            get => actor.AnimationController.IsVisibility;
            set => actor.AnimationController.IsVisibility = value;
        }

        IAWActor actor = null!;

        public virtual void Awake()
        {
            actor = gameObject.GetComponentInParent<IAWActor>();
            Debug.Assert(actor != null);

            if (ForceUnitScale == true)
            {
                Debug.Assert(transform.localScale == Vector3.one, "ForceUnitScale is true. Scale is forced to (1,1,1).");
                transform.localScale = Vector3.one;
            }
        }

        // public virtual async UniTask<int> InitializeAsync(IAWActor actor, SpineAnimationController spineAnimationController, CancellationToken cancellationToken)
        // {
        //     if (actor == null) return GeneralReturnCode.Failed;
        //     return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        // }

        public virtual void Restore()
        {
            ResetScale();
        }

        // // From IAWActorDisplay
        // public virtual void DoUpdate(float deltaTime)
        // {
        // }

        public void ResetScale()
        {
            if (ForceUnitScale == true)
            {
                transform.localScale = Vector3.one;
            }
        }


        public bool IsScaleSign(ValueSignType valueSignType)
        {
            return valueSignType switch
            {
                ValueSignType.PositiveX => transform.localScale.x > 0.0f,
                ValueSignType.NegativeX => transform.localScale.x < 0.0f,
                ValueSignType.PositiveY => transform.localScale.y > 0.0f,
                ValueSignType.NegativeY => transform.localScale.y < 0.0f,
                ValueSignType.PositiveZ => transform.localScale.z > 0.0f,
                ValueSignType.NegativeZ => transform.localScale.z < 0.0f,
                _ => throw new System.NotImplementedException(),
            };
        }

        public void SetScaleSign(ValueSignType valueSignType)
        {
            var scale = transform.localScale;

            switch (valueSignType)
            {
                case ValueSignType.PositiveX:
                    if (scale.x < 0.0f) scale.x = -scale.x;
                    break;
                case ValueSignType.NegativeX:
                    if (scale.x > 0.0f) scale.x = -scale.x;
                    break;
                case ValueSignType.PositiveY:
                    if (scale.y < 0.0f) scale.y = -scale.y;
                    break;
                case ValueSignType.NegativeY:
                    if (scale.y > 0.0f) scale.y = -scale.y;
                    break;
                case ValueSignType.PositiveZ:
                    if (scale.z < 0.0f) scale.z = -scale.z;
                    break;
                case ValueSignType.NegativeZ:
                    if (scale.z > 0.0f) scale.z = -scale.z;
                    break;
                default:
                    Debug.Assert(false, "Unknown ValueSignType");
                    break;
            }

            transform.localScale = scale;
        }

    }
}
#nullable restore