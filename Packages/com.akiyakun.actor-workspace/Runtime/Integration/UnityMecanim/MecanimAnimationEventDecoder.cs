#nullable enable
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ActorWorkspace.UnitySpine
{
    public class MecanimAnimationEventDecoder : IAWAnimationEventDecoder<UnityEngine.AnimationEvent>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AWAnimationEventData Decode(UnityEngine.AnimationEvent rawData)
        {
            return new AWAnimationEventData
            {
                Name = rawData.functionName,
                Int = rawData.intParameter,
                Float = rawData.floatParameter,
                String = rawData.stringParameter,
            };
        }
    }
}
#nullable restore