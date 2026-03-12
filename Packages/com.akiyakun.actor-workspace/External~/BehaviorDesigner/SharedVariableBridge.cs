using afl.BehaviorTask;
using BehaviorDesigner.Runtime;

namespace Project
{
    public class SharedVariableBridge<TValue> : BTSharedVariable<TValue>
    // where TVariable : SharedVariable<TValue>
    {
        public SharedVariable<TValue> sharedVariable;

        public override TValue Value
        {
            get => sharedVariable.Value;
            set => sharedVariable.Value = value;
        }

        public SharedVariableBridge()
        {

        }
        public SharedVariableBridge(SharedVariable<TValue> variable)
        {
            this.sharedVariable = variable;
        }

        public override object GetSharedValue() => sharedVariable.Value;
        public override void SetSharedValue(object value) => sharedVariable.Value = (TValue)value;
        public override bool IsNone => sharedVariable.IsNone;

    }
}
