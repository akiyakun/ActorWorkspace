using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using System.Linq;

namespace ActorWorkspace.UnitySpine
{
    using ImplAnimtionController = SpineSkeletonAnimation;
    public class SpineSkeletonActor : MonoBehaviour, IAWActor
    {
        public GameObject GameObject => this.gameObject;
        // public IAWAnimation Animation { get; protected set; }
        public IAWAnimationController AnimationController { get; protected set; }
        public IReadOnlyList<IAWSkin> SkinList => skinList.Cast<IAWSkin>().ToList();

        SkeletonAnimation skeletonAnimation;
        // ImplAnimtion implAnimtion;
        SpineSkeletonAnimationController spineSkeletonAnimationController;
        // ImplSkin implSkin;
        List<SpineSkin> skinList;

        // FIXME: Awakeよくない
        void Awake()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            Debug.Assert(skeletonAnimation != null);

            // implAnimtion = new SpineSkeletonAnimation(integrationData);
            // Animation = implAnimtion;
            spineSkeletonAnimationController = new SpineSkeletonAnimationController(skeletonAnimation);
            AnimationController = spineSkeletonAnimationController as IAWAnimationController;

            var skins = skeletonAnimation.Skeleton.Data.Skins.Items;
            skinList = new List<SpineSkin>(skins.Length);
            for (int i = 0; i < skins.Length; i++)
            {
                var skin = new SpineSkin(skins[i]);
                skinList.Add(skin);
            }
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
