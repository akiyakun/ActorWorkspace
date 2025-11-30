using UnityEngine;

namespace ActorWorkspace
{
    // FIXME: ざつにつくった
    [RequireComponent(typeof(IAWActor))]
    public abstract class AudioEventProcessingBase : MonoBehaviour
    {
        protected IAWActor actor;
        protected AudioSource audioSource;


        protected virtual void Awake()
        {
            actor = GetComponent<IAWActor>();
            if (actor == null) return;

            actor.AnimationController.OnAnimationEvent += OnAnimationEvent;

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        protected virtual void Oestroy()
        {
            if (actor != null)
            {
                actor.AnimationController.OnAnimationEvent -= OnAnimationEvent;
            }
        }

        void OnAnimationEvent(IAWAnimation animation, AWAnimationEventData eventData)
        {
            // if (eventData.EventType != AWEventType.Audio) return;
            PlayAudio(animation, eventData);
        }

        public abstract void PlayAudio(IAWAnimation animation, AWAnimationEventData eventData);

    }
}
