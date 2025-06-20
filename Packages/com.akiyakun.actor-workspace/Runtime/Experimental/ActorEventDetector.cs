using UnityEngine;
using UnityEngine.Events;

namespace ActorWorkspace
{
    public class ActorEventDetector : MonoBehaviour, IActorEventTrigger
    {
        [SerializeField] UnityEvent onActorEventTrriger;

        public void OnActorEventTrigger(object userData = null)
        {
            onActorEventTrriger?.Invoke();
        }
    }
}
