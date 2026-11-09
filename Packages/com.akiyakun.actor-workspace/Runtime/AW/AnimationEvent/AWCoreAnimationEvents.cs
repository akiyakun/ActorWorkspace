#nullable enable

namespace ActorWorkspace
{
    public static class AWCoreAnimationEvents
    {
        public const string MotionConfig = "MotionConfig";
        public const string Audio = "Audio";
        public const string Effect = "Effect";
        // public const string InterruptBegin = "InterruptBegin";
        // public const string InterruptEnd = "InterruptEnd";
        public const string UninterruptBegin = "UninterruptBegin";
        public const string UninterruptEnd = "UninterruptEnd";
        public const string DisableRootMotionXBegin = "DisableRootMotionXBegin";
        public const string DisableRootMotionXEnd = "DisableRootMotionXEnd";
        public const string EnableRootMotionYBegin = "EnableRootMotionYBegin";
        public const string EnableRootMotionYEnd = "EnableRootMotionYEnd";
        // public const string DisableGroundCheckBegin = "DisableGroundCheckBegin";
        // public const string DisableGroundCheckEnd = "DisableGroundCheckEnd";
        public const string EnableGroundCheckBegin = "EnableGroundCheckBegin";
        public const string EnableGroundCheckEnd = "EnableGroundCheckEnd";
        public const string DisableRootMotionRotationBegin = "DisableRootMotionRotationBegin";
        public const string DisableRootMotionRotationEnd = "DisableRootMotionRotationEnd";

        // HitBox に攻撃を持たせる(int: 攻撃の番号。AWAttackBinding.LocalId)。次のアニメーションに入ると外れる
        // MEMO: ArcaneLedger の ALAttackBindingBehaviour が受ける
        public const string SetAttack = "SetAttack";


        #region General Events
        public const int MaxGeneralEventCount = 'Z' - 'A' + 1;
        public const string SetGeneralEventPrefix = "GeneralEvent";
        #endregion// General Events


        #region General Parameter Events
        public const int MaxGenParamCount = 'Z' - 'A' + 1;
        public const string SetGenPrefix = "SetGen";
        public const string SetGenValuePrefix = SetGenPrefix + "Value";
        public const string SetGenFactorPrefix = SetGenPrefix + "Factor";
        public const string SetGenDeltaPrefix = SetGenPrefix + "Delta";
        #endregion// General Parameter Events

    }
}

#nullable restore