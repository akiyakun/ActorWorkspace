#nullable enable
using System.Runtime.CompilerServices;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;

namespace ActorWorkspace.ArcaneLedger
{
    // FIXME: 適当な名前
    public interface IALProcessor
    {
        public IAWActor Actor { get; }

        public void Setup(IAWActor actor);
        public void Restore();

        public ALGeneralParam GetGeneralParam(int paramId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Calc(int paramId);

        public void SetupStatusEffects(IStatusEffect[] statusEffects);

    }
}
#nullable restore