#nullable enable
using System.Runtime.CompilerServices;

namespace ActorWorkspace
{
    // ExtensionMethods
    public static class IAWAnimationControllerEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEmptyAnimation(this IAWAnimationController self, int track)
        {
            self.SetEmptyAnimation(new AWAnimationOption { Track = track });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAWTrack? SetAnimation(this IAWAnimationController self, int nameHash, bool loop)
        {
            return self.SetAnimation(nameHash, new AWAnimationOption { Flags = loop ? (uint)AWAnimationOptionFlag.Loop : 0 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAWTrack? SetAnimation(this IAWAnimationController self, string name, bool loop)
        {
            return self.SetAnimation(name, new AWAnimationOption { Flags = loop ? (uint)AWAnimationOptionFlag.Loop : 0 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAWTrack? SetAnimation(this IAWAnimationController self, int nameHash, bool loop, bool immediate)
        {
            return self.SetAnimation(nameHash, new AWAnimationOption { Flags = (uint)((loop ? AWAnimationOptionFlag.Loop : 0) | (immediate ? AWAnimationOptionFlag.Immediate : 0)) });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAWTrack? SetAnimation(this IAWAnimationController self, string name, bool loop, bool immediate)
        {
            return self.SetAnimation(name, new AWAnimationOption { Flags = (uint)((loop ? AWAnimationOptionFlag.Loop : 0) | (immediate ? AWAnimationOptionFlag.Immediate : 0)) });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAWTrack? SetAnimation(this IAWAnimationController self, int nameHash, bool loop, int track)
        {
            return self.SetAnimation(nameHash, new AWAnimationOption { Track = track, Flags = loop ? (uint)AWAnimationOptionFlag.Loop : 0 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAWTrack? SetAnimation(this IAWAnimationController self, string name, bool loop, int track)
        {
            return self.SetAnimation(name, new AWAnimationOption { Track = track, Flags = loop ? (uint)AWAnimationOptionFlag.Loop : 0 });
        }
    }
}
#nullable restore