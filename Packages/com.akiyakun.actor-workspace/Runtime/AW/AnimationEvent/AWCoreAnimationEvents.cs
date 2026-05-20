using System;
using UnityEngine;

namespace ActorWorkspace
{
    public class AWCoreAnimationEvents
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



        #region General parameter events
        public const int MaxGenParamCount = 'Z' - 'A' + 1;
        public const string SetGenPrefix = "SetGen";
        public const string SetGenValuePrefix = SetGenPrefix + "Value";
        public const string SetGenFactorPrefix = SetGenPrefix + "Factor";
        public const string SetGenModifierPrefix = SetGenPrefix + "Modifier";

        // public static string GetSetGenValueEventName(int index)
        // {
        //     if (index < 0 || index >= MaxGenParamCount) throw new ArgumentOutOfRangeException(nameof(index));
        //     return SetGenValuePrefix + (char)('A' + index);
        // }

        // public const string SetGenValueA = SetGenValuePrefix + "A";
        // public const string SetGenValueB = SetGenValuePrefix + "B";
        // public const string SetGenValueC = SetGenValuePrefix + "C";
        // public const string SetGenValueD = SetGenValuePrefix + "D";
        // public const string SetGenValueE = SetGenValuePrefix + "E";

        // public const string SetGenFactorA = SetGenFactorPrefix + "A";
        // public const string SetGenFactorB = SetGenFactorPrefix + "B";
        // public const string SetGenFactorC = SetGenFactorPrefix + "C";
        // public const string SetGenFactorD = SetGenFactorPrefix + "D";
        // public const string SetGenFactorE = SetGenFactorPrefix + "E";

        // public const string SetGenModifierA = SetGenModifierPrefix + "A";
        // public const string SetGenModifierB = SetGenModifierPrefix + "B";
        // public const string SetGenModifierC = SetGenModifierPrefix + "C";
        // public const string SetGenModifierD = SetGenModifierPrefix + "D";
        // public const string SetGenModifierE = SetGenModifierPrefix + "E";
        #endregion

    }
}
