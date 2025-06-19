using UnityEngine;
using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace ActorWorkspace
{
    public class SpineAudioFromStreamingAssets : MonoBehaviour
    {
        public enum KeyMode { EventName, AudioPath }
        public KeyMode keyMode = KeyMode.AudioPath;

        public AudioSource audioSource;
        // public string defaultExtension = ".mp3"; // または ".wav" に変更可能

        private HashSet<string> loadingKeys = new HashSet<string>();

        void Awake()
        {
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            var skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation != null)
                skeletonAnimation.AnimationState.Event += HandleEvent;
        }

        void HandleEvent(TrackEntry entry, Spine.Event e)
        {
            string key = keyMode == KeyMode.AudioPath ? e.Data.AudioPath : e.Data.Name;

            if (!string.IsNullOrEmpty(key) && !loadingKeys.Contains(key))
            {
                StartCoroutine(LoadAndPlayAudio(key));
            }
        }

        IEnumerator LoadAndPlayAudio(string key)
        {
            loadingKeys.Add(key);

            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, key/* + defaultExtension*/);

#if UNITY_ANDROID && !UNITY_EDITOR
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.UNKNOWN);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Failed to load audio from: {filePath} - {www.error}");
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.PlayOneShot(clip);
        }
#else
            string url = "file://" + filePath;
            using (WWW www = new WWW(url))
            {
                yield return www;
                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.LogWarning($"Failed to load audio from: {filePath} - {www.error}");
                }
                else
                {
                    AudioClip clip = www.GetAudioClip();
                    audioSource.PlayOneShot(clip);
                }
            }
#endif

            loadingKeys.Remove(key);
        }
    }
}
