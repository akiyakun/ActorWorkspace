#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // AnimatorのParametersをそのまま操作するかたちの実装
    public class MecanimAnimationParameter : IAWAnimationParameter
    {
        Animator animator;

        public MecanimAnimationParameter(Animator animator)
        {
            this.animator = animator;
        }

        public float GetFloat(string name)
        {
            return animator.GetFloat(name);
        }

        public void SetFloat(string name, float value)
        {
            animator.SetFloat(name, value);
        }

        public int GetInt(string name)
        {
            return animator.GetInteger(name);
        }

        public void SetInt(string name, int value)
        {
            animator.SetInteger(name, value);
        }

        public bool GetBool(string name)
        {
            return animator.GetBool(name);
        }

        public void SetBool(string name, bool value)
        {
            animator.SetBool(name, value);
        }
    }
}
#nullable restore