using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using afl.BehaviorTask;

namespace Project.BD
{
    // 引数は画像パス
    // {SkinColor}にはUnityのスキンに応じて Light か Dark が代入される
    // [TaskIcon("Assets/{SkinColor}_ExampleIcon.png")]
    // [TaskCategory("Project")]
    // [TaskName("左右方向移動")]
    public abstract class ActionBase : Action, IBTOwner
    {
        public abstract int Id { get; }

        public new GameObject GameObject
        {
            get => gameObject;
            set => base.GameObject = value;
        }

        IBTAction btAction = null;
        Dictionary<string, BTSharedVariable> properties = new();

        public override void OnAwake()
        {
            var factory = (BTTaskFactory)GlobalVariables.Instance.GetVariable("BTTaskFactory").GetValue();
            Debug.Assert(factory != null);
            btAction = factory.CreateAction(Id, this);
            if (btAction == null) throw new System.Exception();
            OnSetup();
            btAction.OnAwake();
        }

        protected virtual void OnSetup()
        {
        }

        public override void OnStart() => btAction.OnStart();
        public override TaskStatus OnUpdate() => (TaskStatus)btAction.OnUpdate();
        public override void OnEnd() => btAction.OnEnd();
        public override void OnBehaviorComplete()
        {
            btAction.OnBehaviorComplete();
            btAction = null;
        }


        protected virtual void AddProperty(string key, BTSharedVariable variable)
        {
            properties[key] = variable;
        }

        // From IBTOwner
        public BTSharedVariable GetProperty(string key)
        {
            if (properties.TryGetValue(key, out var variable) == false)
            {
                Debug.LogError($"Property not found. key={key}");
                return null;
            }
            return variable;
        }

        // // From IBTTask
        // public virtual object GetValue(string key)
        // {
        //     SharedVariable variable = Owner.GetVariable(key);
        //     if (variable == null)
        //     {
        //         Debug.LogError($"Variable not found. key={key}");
        //         return null;
        //     }
        //     return variable.GetValue();
        // }

        // // From IBTTask
        // public virtual void SetValue(string key, object value)
        // {
        //     Owner.SetVariableValue(key, value);
        // }
    }
}
