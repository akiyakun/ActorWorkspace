#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    public interface IAWAnimationParameter
    {
        public void ResetAll();

        public float GetFloat(int nameHash);
        public float GetFloat(string name);
        public void SetFloat(int nameHash, float value);
        public void SetFloat(string name, float value);

        public int GetInt(int nameHash);
        public int GetInt(string name);
        public void SetInt(int nameHash, int value);
        public void SetInt(string name, int value);

        public bool GetBool(int nameHash);
        public bool GetBool(string name);
        public void SetBool(int nameHash, bool value);
        public void SetBool(string name, bool value);

        // MEMO: Trigger はどこかに遷移したら必ず全てのTriggerがリセットされる想定
        public void ResetTrigger(int nameHash);
        public void ResetTrigger(string name);
        public void SetTrigger(int nameHash);
        public void SetTrigger(string name);
    }
}
#nullable restore