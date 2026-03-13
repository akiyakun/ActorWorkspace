using System.Collections.Generic;
using afl.BehaviorTask;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Project.BD
{
    public abstract class DecoratorBase : Decorator, IBTOwner
    {
        public abstract int Id { get; }

        public new GameObject GameObject
        {
            get => gameObject;
            set => base.GameObject = value;
        }

        // public GameObject Main { get; protected set; }

        IBTDecorator btDecorator = null;
        Dictionary<string, BTSharedVariable> properties = new();

        public override void OnAwake()
        {
            var factory = (BTTaskFactory)GlobalVariables.Instance.GetVariable("BTTaskFactory").GetValue();
            Debug.Assert(factory != null);
            btDecorator = factory.CreateDecorator(Id, this);
            if (btDecorator == null) throw new System.Exception();
            OnSetup();
            btDecorator.OnAwake();
        }

        protected virtual void OnSetup()
        {
        }

        public override void OnStart() => btDecorator.OnStart();
        public override TaskStatus OnUpdate() => (TaskStatus)btDecorator.OnUpdate();
        public override void OnEnd() => btDecorator.OnEnd();
        public override void OnBehaviorComplete()
        {
            btDecorator.OnBehaviorComplete();
            btDecorator = null;
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


        // 実行可能状態か
        public override bool CanExecute() => btDecorator.CanExecute();

        // 子Taskの最大数
        public override int MaxChildren() => base.MaxChildren();

        // 子Taskを並列実行できるか
        public override bool CanRunParallelChildren() => base.CanRunParallelChildren();

        // 子Taskが実行されたときに呼ばれる
        public override void OnChildExecuted(int childIndex, TaskStatus childStatus) => base.OnChildExecuted(childIndex, childStatus);

        // 子Taskが開始した時に呼ばれる
        public override void OnChildStarted() => base.OnChildStarted();

    }
}
