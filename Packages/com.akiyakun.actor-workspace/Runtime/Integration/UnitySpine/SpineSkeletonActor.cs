using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using System.Linq;

namespace ActorWorkspace.UnitySpine
{
    public class SpineSkeletonActor : MonoBehaviour, IAWActor
    {
        public int ActorCategory { get; set; }
        public GameObject GameObject => this.gameObject;
        public IAWActorParam IActorParam { get; set; }

        public IAWAnimationController AnimationController { get; protected set; }
        public IReadOnlyList<IAWSkin> SkinList => skinList.Cast<IAWSkin>().ToList();

        SkeletonAnimation skeletonAnimation;
        IAWEventDecoder eventDecoder;
        SpineSkeletonAnimationController spineSkeletonAnimationController;
        List<SpineSkin> skinList;

        // FIXME: Awakeよくない
        // eventDecoder も受け取りたい
        void Awake()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            Debug.Assert(skeletonAnimation != null);

            eventDecoder = new SpineEventDecoder();
            Debug.Assert(eventDecoder != null);

            spineSkeletonAnimationController = new SpineSkeletonAnimationController(skeletonAnimation, eventDecoder);
            AnimationController = spineSkeletonAnimationController as IAWAnimationController;

            var skins = skeletonAnimation.Skeleton.Data.Skins.Items;
            skinList = new List<SpineSkin>(skins.Length);
            for (int i = 0; i < skins.Length; i++)
            {
                var skin = new SpineSkin(skins[i]);
                skinList.Add(skin);
            }
        }

        public void Restore()
        {
            IActorParam?.Restore();
        }

        public void SetSkin(int skinIndex)
        {
            if (skinIndex < 0 || skinList.Count <= skinIndex)
            {
                Debug.Assert(false, $"Invalid skin index: {skinIndex}");
                return;
            }

            var skin = skinList[skinIndex];

            var skeleton = skeletonAnimation.Skeleton;
            skeleton.SetSkin(skin.Skin);
            skeleton.SetSlotsToSetupPose();
        }
    }
}
