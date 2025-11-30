using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    public interface IAWEventDecoder
    {
        public AWEventData Decode(object rawData);
    }
}
