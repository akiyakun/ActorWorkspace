using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // fixme: interface
    public class IAWTrack
    {
        public static int MaxTrack = 4;

        public virtual float TimeScale { get; set; }
        public virtual float MixDuration { get; set; }

        public virtual IAWAnimation Animation { get; protected set; }
    }
}
