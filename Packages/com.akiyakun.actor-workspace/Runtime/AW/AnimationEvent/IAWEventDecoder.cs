#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    public interface IAWAnimationEventDecoder<T>
    {
        public AWAnimationEventData Decode(T rawData);
    }
}
#nullable restore