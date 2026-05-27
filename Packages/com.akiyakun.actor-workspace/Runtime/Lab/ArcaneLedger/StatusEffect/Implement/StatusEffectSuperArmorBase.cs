#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace.ArcaneLedger
{
    public abstract class StatusEffectSuperArmorBase : StatusEffect
    {
        public override int Id => (int)CoreStatusEffectId.SuperArmor;
    }
}
#nullable restore