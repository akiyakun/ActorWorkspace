using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    using IntegrationData = SkeletonAnimation;
    using ImplAnimtionController = SpineSkeletonAnimation;
    // using ImplSkin = SpineSkeletonAnimation;
    public class SpineSkeletonActor : MonoBehaviour, IAWActor
    {
        public GameObject GameObject => this.gameObject;
        // public IAWAnimation Animation { get; protected set; }
        public IAWAnimationController AnimationController { get; protected set; }
        public IReadOnlyList<IAWSkin> SkinList => skinList;

        IntegrationData integrationData;
        // ImplAnimtion implAnimtion;
        ImplAnimtionController implAnimtionController;
        // ImplSkin implSkin;
        List<IAWSkin> skinList;

        void Awake()
        {
            integrationData = GetComponent<SkeletonAnimation>();
            Debug.Assert(integrationData != null);

            // implAnimtion = new SpineSkeletonAnimation(integrationData);
            // Animation = implAnimtion;
            implAnimtionController = new SpineSkeletonAnimationController(integrationData);

            skinList = new List<IAWSkin>();
        }
    }
}
