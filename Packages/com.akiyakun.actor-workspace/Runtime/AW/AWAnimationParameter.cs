#nullable enable
using System.Collections.Generic;
using afl;
using UnityEngine;

namespace ActorWorkspace
{
    // FIXME: とりあえずVariableTableで簡単に実装
    public class AWAnimationParameter : IAWAnimationParameter
    {
        VariableTable variableTable = new();

        public float GetFloat(string name) => variableTable.Get(name).GetFloat();
        public void SetFloat(string name, float value) =>variableTable.Get(name).SetFloat(value);
        public int GetInt(string name) => variableTable.Get(name).GetInt();
        public void SetInt(string name, int value) => variableTable.Get(name).SetInt(value);
        public bool GetBool(string name) => variableTable.Get(name).GetBool();
        public void SetBool(string name, bool value) => variableTable.Get(name).SetBool(value);
    }
}
#nullable restore