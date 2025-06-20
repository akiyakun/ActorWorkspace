using UnityEngine;

namespace ActorWorkspace
{
    public interface IActorEventTrigger
    {
        void OnActorEventTrigger(object userData = null);
    }
}
