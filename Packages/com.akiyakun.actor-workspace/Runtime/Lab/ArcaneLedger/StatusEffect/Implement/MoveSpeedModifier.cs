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

        protected override void Awake()
        {
            base.Awake();
            factorVariable = Actor.Variables.Get(FactorVariableName);
        }
        public override void Restore()
        {
            base.Restore();
            factorVariable.Float = 0.0f;
        }

        public override void Prepare()
        {
            factorVariable.Float = 0.0f;
        }

        protected override void OnUpdate(float deltaTime)
        {
            // 既に設定されているときは優先度が高い効果のほうを優先する
            if (factorVariable.Float != 0.0f) return;

            factorVariable.Float = Params[(int)Param.MoveSpeedFactor].Float;
        }

    }
}
#nullable restore