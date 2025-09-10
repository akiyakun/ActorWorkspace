#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.Tests
{
    public class StubAWTrack : IAWTrack
    {
        public int TrackIndex { get; protected set; }
        public float TimeScale { get; set; }
        public float MixDuration { get; set; }

        public IAWAnimation? Animation { get; protected set; }

        protected StubAWTrack() { }
        public StubAWTrack(int trackIndex)
        {
            TrackIndex = trackIndex;
        }

        // public void Set(FakeAWTrack track)
        // {
        //     Animation = track as IAWAnimation;
        // }
    }
}
#nullable restore