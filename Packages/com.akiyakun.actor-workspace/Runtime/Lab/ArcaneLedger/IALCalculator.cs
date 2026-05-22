#nullable enable
using System.Runtime.CompilerServices;

namespace ActorWorkspace.ArcaneLedger
{
    public interface IALCalculator
    {
        public void Restore();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Calc(int paramId);

    }
}
#nullable restore