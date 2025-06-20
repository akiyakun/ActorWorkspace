using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    public class SpineSkeletonActor : MonoBehaviour, IAWActor
    {
        public GameObject GameObject => this.gameObject;
    }
}
