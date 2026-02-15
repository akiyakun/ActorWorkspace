using System.Collections.Generic;
using afl.BehaviorTask;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Project.BD
{
    public abstract class ConditionalBase : Conditional, IBTOwner
    {
        public abstract int Id { get; }

        public new GameObject GameObject
        {
            get => gameObject;
            set => base.GameObject = value;
        }

        IBTConditional btConditional = null;
        Dictionary<string, BTSharedVariable> properties = new();

        public override void OnAwake()
        {
            var factory = (BTTaskFactory)GlobalVariables.Instance.GetVariable("BTTaskFactory").GetValue();
            Debug.Assert(factory != null);
            btConditional = factory.CreateConditional(Id, this);
            if (btConditional == null) throw new System.Exception();
            OnSetup();
            btConditional.OnAwake();
        }

        protected virtual void OnSetup()
        {
        }

        public override void OnStart() => btConditional.OnStart();
        public override TaskStatus OnUpdate() => (TaskStatus)btConditional.OnUpdate();
        public override void OnEnd() => btConditional.OnEnd();
        public override void OnBehaviorComplete()
        {
            btConditional.OnBehaviorComplete();
            btConditional = null;
        }


        // // From IBTOwner
        // public virtual object GetValue(string key)
        // {
        //     return btConditional.GetValue(key);
        // }

        // // From IBTOwner
        // public virtual void SetValue(string key, object value)
        // {
        //     btConditional.SetValue(key, value);
        // }


        // // From IBTOwner
        // public virtual BTSharedVariable CreateSharedVariable(string key)
        // {
        //     SharedVariable variable = Owner.GetVariable(key);
        //     if (variable == null)
        //     {
        //         Debug.LogError($"Variable not found. key={key}");
        //         return null;
        //     }
        //     var type = variable.GetType();
        //     var genericArgs = type.GetGenericArguments();
        //     var bridgeType = typeof(SharedVariableBridge<,>).MakeGenericType(genericArgs[0], type);
        //     var bridge = (BTSharedVariable)System.Activator.CreateInstance(bridgeType, variable);
        //     return bridge;
        // }


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

        // From IBTOwner
        // public virtual object GetSharedVariable(string key)
        // {
        //     SharedVariable variable = Owner.GetVariable(key);
        //     if (variable == null)
        //     {
        //         Debug.LogError($"Variable not found. key={key}");
        //         return null;
        //     }
        //     return variable;
        // }

        // From IBTOwner
        // public virtual void SetSharedVariable(string key, object variable)
        // {
        //     Owner.SetVariableValue(key, variable);
        // }
    }
}
