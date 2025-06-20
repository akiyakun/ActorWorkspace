using UnityEngine;
using Spine;
using Spine.Unity;

namespace ActorWorkspace
{
    public class SpineAudioFromStreamingAssets : MonoBehaviour
    {
        // public enum KeyMode { EventName, AudioPath }
        // public KeyMode keyMode = KeyMode.AudioPath;

        public AudioSource audioSource;
        ResourcesAssetLoader<AudioClip> assetLoader = new("Audio");

        void Awake()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            var skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation != null)
            {
                skeletonAnimation.AnimationState.Event += HandleEvent;
            }
        }

        void HandleEvent(TrackEntry entry, Spine.Event e)
        {
            if (string.IsNullOrEmpty(e.Data.AudioPath)) return;

            // string key = keyMode switch
            // {
            //     KeyMode.AudioPath => System.IO.Path.GetFileNameWithoutExtension(e.Data.AudioPath),
            //     _ => e.Data.Name
            // };

            string key = System.IO.Path.GetFileNameWithoutExtension(e.Data.AudioPath);

            var asset = assetLoader.LoadAsset(key);
            if (asset != null)
            {
                audioSource.PlayOneShot(asset);
            }
        }
    }
}
