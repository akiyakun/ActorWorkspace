#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // アクターの表示のルート
    // 全体をスケールしたり反転したりするよう
    // MonoBehaviour前提
    public class AWActorDisplay : MonoBehaviour, System.IDisposable
    {
        [SerializeField] GameObject main = null!;

        // FIXME:
#if UNITY_EDITOR
        public GameObject Main { get => main; set => main = value; }
#else
        public GameObject Main => main;
#endif

        public bool ForceUnitScale { get; set; } = true;

        public bool IsVisibility
        {
            get => Actor.AnimationController.IsVisibility;
            set => Actor.AnimationController.IsVisibility = value;
        }

        protected IAWActor Actor { get; private set; } = null!;

        // 反転を打ち消す子(3D の当たり判定など)。スケールを変えたら Apply() を呼ぶ
        AWKeepPositiveScale[] keepPositiveScales = System.Array.Empty<AWKeepPositiveScale>();

        // Actorの初期化処理の最後に呼ばれます
        public virtual async UniTask<int> InitializeAsync(IAWActor actor, CancellationToken cancellationToken)
        {
            Actor = actor;

            Debug.Assert(main != null, $"Main is not assigned. ActorId={Actor.ActorId}, name={Actor.GameObject.name}");

            // MEMO: ここより後に足した子(初期化のあとで作った当たり判定など)は対象にならない
            keepPositiveScales = GetComponentsInChildren<AWKeepPositiveScale>(includeInactive: true);

            if (ForceUnitScale == true)
            {
                Debug.Assert(transform.localScale == Vector3.one, "ForceUnitScale is true. Scale is forced to (1,1,1).");
                transform.localScale = Vector3.one;
            }
            ApplyKeepPositiveScales();

            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        public virtual void Dispose()
        {
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
                ApplyKeepPositiveScales();
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

            // MEMO: 向きが変わらないときも毎フレーム呼ばれることがあるので、変わったときだけ子を直す
            if (scale == transform.localScale) return;

            transform.localScale = scale;
            ApplyKeepPositiveScales();
        }

        void ApplyKeepPositiveScales()
        {
            for (int i = 0; i < keepPositiveScales.Length; ++i)
            {
                // MEMO: 初期化のあとで子を Destroy したときのために null を確かめる
                if (keepPositiveScales[i] != null) keepPositiveScales[i].Apply();
            }
        }

    }
}
#nullable restore