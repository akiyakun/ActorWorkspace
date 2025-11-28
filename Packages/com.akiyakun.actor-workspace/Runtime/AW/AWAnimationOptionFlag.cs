#nullable enable
namespace ActorWorkspace
{
    public enum AWAnimationOptionFlag : uint
    {
        None = 0,
        Loop = 1 << 0,
        // RootMotion = 1 << 1,
        // NoRootMotion = 1 << 2,
        Immediate = 1 << 3,
        // Additive = 1 << 4,
    }
}
#nullable restore