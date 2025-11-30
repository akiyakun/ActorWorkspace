#nullable enable
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ActorWorkspace.UnitySpine
{
    // EventName
    // EventName[param]
    // [param]
    public class SpineMecanimAnimationEventDecoder : IAWAnimationEventDecoder<UnityEngine.AnimationEvent>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AWAnimationEventData Decode(UnityEngine.AnimationEvent rawData)
        {
            // Debug.Log($"Decode AnimationEvent: {rawData.stringParameter}");

            // if (rawData.stringParameter == "Event_03[event3]")
            // {
            //     Debug.Log("Decode AnimationEvent: Event_All detected.");
            // }

            string str = rawData.stringParameter;
            Debug.Assert(object.ReferenceEquals(str, rawData.stringParameter));
            Debug.Assert(string.IsNullOrEmpty(str) == false);

            int colonIndex = -1;
            // if (string.IsNullOrEmpty(str) == false && str[0] != '[')
            // if (str[0] != '[')
            {
                colonIndex = str.IndexOf('[');
                // if (colonIndex <= 0) throw new Exception("Invalid stringParameter format. Missing closing ']'.");
            }
            // else
            // {
            //     colonIndex = 0;
            // }

            // ReadOnlySpan<char> s = rawData.stringParameter.AsSpan(0, colonIndex)

            // var ame = colonIndex < 0 ? str : str.Substring(0, colonIndex);
            // int a = colonIndex + 1;
            // int l = str.Length;
            // int b = str.Length - colonIndex;
            // var tring = colonIndex < 0 ? string.Empty : str.Substring(colonIndex + 1, str.Length - (colonIndex + 2));

#if __DEBUG__
            if (colonIndex >= 0)
            {
                Debug.Assert(str.IndexOf(']') > 0);
            }
#endif

            return new AWAnimationEventData
            {
                Name = colonIndex < 0 ? str : str.Substring(0, colonIndex),
                // Name = colonIndex > 0 ? str.AsSpan(1, colonIndex) : rawData.functionName,
                Int = rawData.intParameter,
                Float = rawData.floatParameter,
                String = colonIndex < 0 ? string.Empty : str.Substring(colonIndex + 1, str.Length - (colonIndex + 2)),
                // String = colonIndex > 0 ? str.AsSpan(colonIndex + 1, str.Length - colonIndex + 1) : str,
            };
        }
    }
}
#nullable restore