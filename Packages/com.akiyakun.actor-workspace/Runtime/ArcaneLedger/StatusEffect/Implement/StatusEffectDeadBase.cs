#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace.ArcaneLedger
{
    public abstract class StatusEffectDeadBase : StatusEffect
    {
        public override int Id => (int)CoreStatusEffectId.Dead;
    }
}
#nullable restore