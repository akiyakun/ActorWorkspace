using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using System.Linq;

namespace ActorWorkspace.UnitySpine
{
    public class SpineSkeletonActor : MonoBehaviour, IAWActor
    {
        public virtual int ActorCategory { get; protected set; }
        public virtual GameObject GameObject => this.gameObject;
        public virtual IAWActorParam IActorParam { get; protected set; }

        public IAWActorDisplay ActorDisplay { get; protected set; }
        public IAWAnimationController AnimationController { get; protected set; }
        public IReadOnlyList<IAWSkin> SkinList => skinList.Cast<IAWSkin>().ToList();

        SkeletonAnimation skeletonAnimation;
        IAWEventDecoder eventDecoder;
        SpineSkeletonAnimationController spineSkeletonAnimationController;
        List<SpineSkin> skinList;

        // FIXME: Awakeよくない
        // eventDecoder も受け取りたい
        protected virtual void Awake()
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            Debug.Assert(skeletonAnimation != null, "SkeletonAnimation component not found in children.");

            eventDecoder = new SpineEventDecoder();
            Debug.Assert(eventDecoder != null);

            {
                // var skeletonMecanim = GetComponent<SkeletonMecanim>();
                // var skeletonAnimation = GetComponent<SkeletonAnimation>();

                ActorDisplay = new SpineActorDisplay(skeletonAnimation);
            }

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
