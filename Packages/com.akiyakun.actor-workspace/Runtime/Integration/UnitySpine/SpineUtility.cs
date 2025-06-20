using System.Runtime.CompilerServices;
using UnityEngine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    public static class ActorEventBuiltInName
    {
        public const string Emitter = "emitter";
        // public const string Audio = "audio";
    }

    public static partial class SpineUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ParseEventName(string value)
        {
            int index = value.IndexOf('=');
            if (index < 0) return value;
            return value.Substring(0, index);
        }
    }
}
