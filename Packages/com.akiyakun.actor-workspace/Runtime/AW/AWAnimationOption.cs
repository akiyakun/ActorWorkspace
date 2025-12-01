#nullable enable
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    public struct AWAnimationOption
    {
        // public static AWAnimationOption Default = new AWAnimationOption();

        // public string Name;
        public int Track;
        public uint Flags;

        // 0.0f の場合はデフォルトの長さ
        public float Duration;

        public bool Loop => HasFlag(AWAnimationOptionFlag.Loop);
        public bool Immediate => HasFlag(AWAnimationOptionFlag.Immediate);
        // public bool Parameter => HasFlag(AWAnimationOptionFlag.Parameter);

        // public AWAnimationOption()
        // {
        //     Duration = -1.0f;
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasFlag(AWAnimationOptionFlag flag) => (Flags & (uint)flag) != 0;

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static AWAnimationOption New(int track) => new AWAnimationOption { Track = track };
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static AWAnimationOption New(int track, bool loop) => new AWAnimationOption { Track = track, Flags = loop ? (uint)AWAnimationOptionFlag.Loop : 0 };
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static AWAnimationOption New(bool loop) => new AWAnimationOption { Flags = loop ? (uint)AWAnimationOptionFlag.Loop : 0 };

    }
}
#nullable restore