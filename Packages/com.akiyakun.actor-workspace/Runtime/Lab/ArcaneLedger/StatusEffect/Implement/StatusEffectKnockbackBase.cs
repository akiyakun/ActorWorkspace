#nullable enable
using UnityEngine;
using ActorWorkspace.ArcaneLedger;
using afl;

namespace ActorWorkspace.ArcaneLedger
{
    public abstract class StatusEffectKnockbackBase : StatusEffect
    {
        public override int Id => (int)CoreStatusEffectId.Knockback;
    }
}
#nullable restore