using UnityEngine;

namespace ActorWorkspace
{
    [RequireComponent(typeof(IAWActor))]
    public class AudioEventProcessingFromResources : AudioEventProcessingBase
    {
        static ResourcesAssetLoader<AudioClip> assetLoader = new("Audio");

        public override void PlayAudio(IAWAnimation animation, AWAnimationEventData eventData)
        {
            string path = System.IO.Path.GetFileNameWithoutExtension(eventData.String);
            AudioClip clip = assetLoader.LoadAsset(path);
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"Audio not found: {path}");
            }
        }
    }
}
