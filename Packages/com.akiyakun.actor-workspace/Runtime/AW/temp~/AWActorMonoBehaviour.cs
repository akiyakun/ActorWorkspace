using UnityEngine;

namespace ActorWorkspace
{
    public class AWActorMonoBehaviour : MonoBehaviour, IAWActor
    {
        public virtual GameObject GameObject => gameObject;
    }
}
