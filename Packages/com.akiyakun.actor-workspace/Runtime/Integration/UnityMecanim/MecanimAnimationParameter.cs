#nullable enable
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // AnimatorのParametersをそのまま操作するかたちの実装
    public class MecanimAnimationParameter : IAWAnimationParameter
    {
        Animator animator;
        List<AnimatorControllerParameter> triggerList = new();

        public MecanimAnimationParameter(Animator animator)
        {
            if (animator == null) throw new System.ArgumentNullException(nameof(animator));
            this.animator = animator;

            // Triggerのリストを作成
            {
                foreach (var param in animator.parameters)
                {
                    if (param.type == AnimatorControllerParameterType.Trigger)
                    {
                        triggerList.Add(param);
                    }
                }
            }
        }

        public void ResetAll()
        {
            foreach (var param in animator.parameters)
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(param.nameHash, param.defaultFloat);
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(param.nameHash, param.defaultInt);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(param.nameHash, param.defaultBool);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        animator.ResetTrigger(param.nameHash);
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetFloat(int nameHash) => animator.GetFloat(nameHash);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetFloat(string name) => animator.GetFloat(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFloat(int nameHash, float value) => animator.SetFloat(nameHash, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFloat(string name, float value) => animator.SetFloat(name, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInt(int nameHash) => animator.GetInteger(nameHash);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInt(string name) => animator.GetInteger(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetInt(int nameHash, int value) => animator.SetInteger(nameHash, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetInt(string name, int value) => animator.SetInteger(name, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetBool(int nameHash) => animator.GetBool(nameHash);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetBool(string name) => animator.GetBool(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBool(int nameHash, bool value) => animator.SetBool(nameHash, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBool(string name, bool value) => animator.SetBool(name, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetTrigger(int nameHash) => animator.ResetTrigger(nameHash);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetTrigger(string name) => animator.ResetTrigger(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTrigger(int nameHash) => animator.SetTrigger(nameHash);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTrigger(string name) => animator.SetTrigger(name);

        public void AllResetTrigger()
        {
            foreach (var param in triggerList)
            {
                animator.ResetTrigger(param.nameHash);
            }
        }
    }
}
#nullable restore