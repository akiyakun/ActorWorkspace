using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    using ImplAnimtionController = SpineSkeletonAnimation;
    public class SpineSkeletonActor : MonoBehaviour, IAWActor
    {
        public GameObject GameObject => this.gameObject;
        // public IAWAnimation Animation { get; protected set; }
        public IAWAnimationController AnimationController { get; protected set; }
        public IReadOnlyList<IAWSkin> SkinList => skinList;

        SkeletonAnimation skeletonAnimation;
        // ImplAnimtion implAnimtion;
        SpineSkeletonAnimationController spineSkeletonAnimationController;
        // ImplSkin implSkin;
        List<IAWSkin> skinList;

        // FIXME: Awakeよくない
        void Awake()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            Debug.Assert(skeletonAnimation != null);

            // implAnimtion = new SpineSkeletonAnimation(integrationData);
            // Animation = implAnimtion;
            spineSkeletonAnimationController = new SpineSkeletonAnimationController(skeletonAnimation);
            AnimationController = spineSkeletonAnimationController as IAWAnimationController;

            skinList = new List<IAWSkin>();
        }
    }
}
