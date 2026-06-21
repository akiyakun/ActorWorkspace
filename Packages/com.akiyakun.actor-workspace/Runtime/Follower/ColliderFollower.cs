#nullable enable
// using System;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // UnityEngine.Collider を継承したコライダー用
    public class ColliderFollower : MonoBehaviour, IAWFollower
    {
        //SphereColliderFollower
        // public SkeletonRenderer skeletonRenderer = null!;

        // From IAWFollower
        public event System.Action<IAWFollower>? OnActivating;

        // From IAWFollower
        public GameObject GameObject => gameObject;

        // [NonSerialized, Disable] public string? FolderName;

        bool isInitialized = false;

        protected virtual void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (isInitialized) return;

            // if (skeletonRenderer == null)
            // {
            //     throw new System.Exception("skeletonRenderer is null");
            // }

            var detector = GetComponent<CollisionDetector>();
            if (detector != null)
            {
                detector.OnCollision += (info) =>
                {
                    Debug.Log($"ColliderFollower OnCollision: EventType={info.EventType}, Other={info.Other.name}");
                };
            }

            isInitialized = true;
        }

        // void OnDestroy()
        // {
        // }

    }
}
#nullable restore