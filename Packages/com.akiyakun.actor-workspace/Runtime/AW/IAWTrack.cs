#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // fixme: interface
    public interface IAWTrack
    {
        public static int MaxTrack = 4;

        public int TrackIndex { get; }

        public float TimeScale { get; set; }
        public float MixDuration { get; set; }

        public IAWAnimation? Animation { get; }

    }
}
#nullable restore