#nullable enable
using System.Runtime.CompilerServices;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;
using afl;

namespace ActorWorkspace.ArcaneLedger
{
    // FIXME: 適当な名前
    public interface IALProcessor
    {
        public IAWActor Actor { get; }

        public void Setup(ArcaneLedgerBehaviour owner, StatusEffectController statusEffectController);
        public void Restore();

        // public ALGeneralParam GetGeneralParam(int paramId);

        public HitResult Hit(CollisionContactInfo contactInfo);

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Calc(int paramId);

        // public void SetupStatusEffects(IStatusEffect[] statusEffects);

    }
}
#nullable restore