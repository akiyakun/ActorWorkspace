using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using ActorWorkspace.ActorAssetDatabase;

using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    public class SpineActorAssetDatabaseForReference : ActorAssetDatabaseForReference
    {
        public override GameObject CreateActorAsset(string path)
        {
            var ret = Create(GetActorAssetId(path)).GetAwaiter().GetResult().GameObject;


            var skeletonAnimation = ret.GetComponent<SkeletonAnimation>();

            // skeletonAnimation.AnimationState.Event += HandleSpineEvent;

            // var c = GetComponent<SpineEventDebuggerGUI>();
            // c.skeletonAnimation = skeletonAnimation;
            // c.Set();

            // var e = GetComponent<SpineEventGizmoVisualizer>();
            // e.skeletonAnimation = skeletonAnimation;

            skeletonAnimation.gameObject.AddComponent<SpineAudioFromStreamingAssets>();
            skeletonAnimation.gameObject.AddComponent<SpineEffectPlayer>();

            return ret;
        }

        // From IAWActorFactory
        public override async UniTask<IAWActor> Create(int id)
        {

            var original = GetActorAssetReference(id);
            if (original == null) return null;

            var skeletonDataAsset = GameObject.Instantiate(original) as SkeletonDataAsset;
            Debug.Assert(skeletonDataAsset != null);

            var newSkeleton = new GameObject(skeletonDataAsset.name);
            newSkeleton.SetActive(false);

            var skeletonAnimation = newSkeleton.AddComponent<SkeletonAnimation>();
            skeletonAnimation.skeletonDataAsset = skeletonDataAsset;
            skeletonAnimation.Initialize(true);

            // skeletonAnimation.skeleton.SetSkin(skins.Items[skinIndex]);
            skeletonAnimation.Skeleton.SetSlotsToSetupPose();

            var spineSkeletonActor = newSkeleton.AddComponent<SpineSkeletonActor>();

            // await UniTask.WaitForSeconds(0.0f);
            newSkeleton.SetActive(true);

            return spineSkeletonActor as IAWActor;
        }





        /*
            使えそうな文字
            moji-moji_moji:[](),.<>/&*@#=moj

            SpineEditorでみやすさを考慮すると「-」や「=」を区切り文字にするのが良さそう
        */
        // void HandleSpineEvent(TrackEntry trackEntry, Spine.Event e)
        // {
        //     ActorEventData actorEventData = new();
        //     actorEventData.Name = ParseEventName(e.Data.Name);

        //     Debug.Log($"Spine Event: {e.Data.Name}, int: {e.Int}, float: {e.Float}, string: {e.String}\nName: {actorEventData.Name}");
        // }

        public class ActorEventData
        {
            // public int Id;
            public string Name;

            public int Int;
            public int Float;
            public string String;
        }

        public static string ParseEventName(string value)
        {
            var result = value.Split('=', System.StringSplitOptions.RemoveEmptyEntries);
            Debug.Assert(result.Length >= 1);
            return result[0];
        }

        public void ExecuteActorEvent(ActorEventData actorEventData)
        {
            switch (actorEventData.Name)
            {
                case "emitter":
                    break;
                case "LoadAsset":
                    break;
                default:
                    Debug.LogWarning($"Unknown Actor Event: {actorEventData.Name}");
                    break;
            }
        }

    }
}
