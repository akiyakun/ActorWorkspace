using UnityEngine;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    public class SpineEffectPlayer : MonoBehaviour
    {
        // 再生位置（未指定なら this）
        public Transform effectRoot;

        ResourcesAssetLoader<GameObject> assetLoader = new("Effect");
        SkeletonAnimation skeletonAnimation;

        void Awake()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation != null)
            {
                skeletonAnimation.AnimationState.Event += HandleEvent;
            }

            if (effectRoot == null)
            {
                effectRoot = this.transform;
            }
        }

        void HandleEvent(TrackEntry entry, Spine.Event e)
        {
            if (SpineUtility.ParseEventName(e.Data.Name) != ActorEventBuiltInName.Emitter) return;

            string key = "";

            // string effectName = e.Data.Name; // または e.String / e.Data.AudioPath など
            // Debug.Log($"SpineEffectPlayer: Name: {e.Data.Name}. Int: {e.Int}, Float: {e.Float}, String: {e.String}");
            var args = e.String.Split(new char[] { '-', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < args.Length; ++i)
            {
                switch (args[i])
                {
                    case "parent":
                        {
                            Debug.Log($"SpineEffectPlayer: Parent: {args[i + 1]}");
                            Bone bone = skeletonAnimation.Skeleton.FindBone(args[i + 1]);
                            Debug.Assert(bone != null);
                            ++i;
                        }
                        break;
                    case "effect_id":
                        Debug.Assert(false);
                        break;
                    case "effect_name":
                        key = args[++i];
                        break;
                }
            }

            var asset = assetLoader.LoadAsset(key);
            if (asset != null)
            {
                GameObject effect = Instantiate(asset, effectRoot.position, Quaternion.identity);

                var particleSystem = effect.GetComponent<ParticleSystem>();

                // 自動削除を設定しておく
                var mainModule = particleSystem.main;
                mainModule.loop = false;
                mainModule.stopAction = ParticleSystemStopAction.Destroy;

                particleSystem.Play();
            }

        }
    }
}
