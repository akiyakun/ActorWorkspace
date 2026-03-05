using afl.BehaviorTask;
using BehaviorDesigner.Runtime;

namespace Project
{
    public class SharedVariableBridge<TValue> : BTSharedVariable<TValue>
    // where TVariable : SharedVariable<TValue>
    {
        SharedVariable<TValue> variable;

        public override TValue Value
        {
            get => variable.Value;
            set => variable.Value = value;
        }

        public SharedVariableBridge(SharedVariable<TValue> variable)
        {
            this.variable = variable;
        }

        public override object GetSharedValue() => variable.Value;
        public override void SetSharedValue(object value) => variable.Value = (TValue)value;
        public override bool IsNone => variable.IsNone;

    }
}
