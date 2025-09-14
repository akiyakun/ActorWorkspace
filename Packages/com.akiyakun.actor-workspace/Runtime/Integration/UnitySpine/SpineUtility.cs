using System.Runtime.CompilerServices;
using UnityEngine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    public static class SpineUtility
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
