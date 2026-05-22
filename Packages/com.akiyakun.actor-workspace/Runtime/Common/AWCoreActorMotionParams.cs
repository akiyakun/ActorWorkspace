#nullable enable
using afl;

namespace ActorWorkspace
{
    public interface AWCoreActorMotionParams
    {
#if __INAPPDEBUG__ == false
        public static int DamageReaction => DamageReactionHash;
#else
        public const string DamageReaction = DamageReactionName;
#endif

        // [int] ダメージリアクションの値
        public const string DamageReactionName = "DamageReaction";
        public static readonly int DamageReactionHash = Utility.StringToHashId(DamageReactionName);

    }
}
#nullable restore