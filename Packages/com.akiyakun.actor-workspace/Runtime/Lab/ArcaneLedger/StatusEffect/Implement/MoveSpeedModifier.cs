#nullable enable
using afl;

namespace ActorWorkspace.ArcaneLedger
{
    /*
        Param1:
    */
    public abstract class MoveSpeedModifier : StatusEffect
    {
        public const string DefaultMoveSpeedFactorVariableName = "StatusEffect_MoveSpeedModifier";
        public virtual string FactorVariableName => DefaultMoveSpeedFactorVariableName;

        public enum Param
        {
            MoveSpeedFactor,
        }

        Variable factorVariable = null!;

        protected override void OnRestore()
        {
            factorVariable.Float = 0.0f;
        }

        protected override void OnAwake()
        {
            factorVariable = Actor.Variables.Get(FactorVariableName);
        }

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }

        protected override void OnPrepare()
        {
            OnRestore();
        }

        protected override void OnUpdate(float deltaTime)
        {
            // 既に設定されているときは優先度が高い効果のほうを優先する
            if (factorVariable.Float != 0.0f) return;

            factorVariable.Float = GeneralParams[(int)Param.MoveSpeedFactor].Float;
        }

    }
}
#nullable restore