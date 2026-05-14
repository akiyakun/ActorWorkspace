using UnityEngine;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedBounds : SharedVariable<Bounds>
    {
        public static implicit operator SharedBounds(Bounds value) { return new SharedBounds { mValue = value }; }
    }
}