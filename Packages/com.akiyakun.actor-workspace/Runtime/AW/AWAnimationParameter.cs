#nullable enable
using System.Collections.Generic;
using afl;
using UnityEngine;

namespace ActorWorkspace
{
    // FIXME: とりあえずVariableTableで簡単に実装
    public class AWAnimationParameter : IAWAnimationParameter
    {
        Dictionary<int, Variable> table = new();

        public void ResetAll()
        {
            foreach (var v in table.Values)
            {
                if (v.Type == VariableType.Int)
                {
                    v.SetInt(0);
                }
                else if (v.Type == VariableType.Float)
                {
                    v.SetFloat(0.0f);
                }
                else if (v.Type == VariableType.Bool)
                {
                    v.SetBool(false);
                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }

        public float GetFloat(int nameHash) => table.Get(nameHash) is Variable v ? v.GetFloat() : 0.0f;
        public float GetFloat(string name) => table.Get(Utility.StringToHashId(name)) is Variable v ? v.GetFloat() : 0.0f;
        public void SetFloat(int nameHash, float value)
        {
            if (table.Get(nameHash) is Variable v)
            {
                v.SetFloat(value);
            }
            else
            {
                v = new Variable();
                v.SetFloat(value);
                table.Add(nameHash, v);
            }
        }
        public void SetFloat(string name, float value) => SetFloat(Utility.StringToHashId(name), value);

        public int GetInt(int nameHash) => table.Get(nameHash) is Variable v ? v.GetInt() : 0;
        public int GetInt(string name) => table.Get(Utility.StringToHashId(name)) is Variable v ? v.GetInt() : 0;
        public void SetInt(int nameHash, int value)
        {
            if (table.Get(nameHash) is Variable v)
            {
                v.SetInt(value);
            }
            else
            {
                v = new Variable();
                v.SetInt(value);
                table.Add(nameHash, v);
            }
        }
        public void SetInt(string name, int value) => SetInt(Utility.StringToHashId(name), value);

        public bool GetBool(int nameHash) => table.Get(nameHash) is Variable v ? v.GetBool() : false;
        public bool GetBool(string name) => table.Get(Utility.StringToHashId(name)) is Variable v ? v.GetBool() : false;
        public void SetBool(int nameHash, bool value)
        {
            if (table.Get(nameHash) is Variable v)
            {
                v.SetBool(value);
            }
            else
            {
                v = new Variable();
                v.SetBool(value);
                table.Add(nameHash, v);
            }
        }
        public void SetBool(string name, bool value) => SetBool(Utility.StringToHashId(name), value);
    }
}
#nullable restore