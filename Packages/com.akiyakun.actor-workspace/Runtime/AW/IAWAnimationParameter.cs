#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    public interface IAWAnimationParameter
    {
        public float GetFloat(string name);
        public void SetFloat(string name, float value);
        public int GetInt(string name);
        public void SetInt(string name, int value);
        public bool GetBool(string name);
        public void SetBool(string name, bool value);
    }
}
#nullable restore