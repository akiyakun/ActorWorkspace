#nullable enable
using UnityEngine;
using afl;
using afl.Service.Input;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    public class ALHitBoxBehaviour : AWActorBehaviour<IAWActor>
    {
        static uint currentSerialNumber = 0;
        public static uint GetNextSerialNumber()
        {
            return unchecked(++currentSerialNumber);
        }

        public override void Restore()
        {

        }

        public override void DoAwake()
        {
            var folderObject = Variables.Get(AWCoreVariableKey.HitBoxFolder).GetGameObject();
            if (folderObject != null)
            {
                var followers = folderObject.GetComponentsInChildren<IAWFollower>(includeInactive: true);
                for (int i = 0; i < followers.Length; i++)
                {
                    var follower = followers[i];

                    var extraInfo = follower.GameObject.AddComponent<ALColliderExtraInfo>();
                    extraInfo.Actor = Actor;

                    EventBag.In(follower,
                        (entity) => entity.OnActivating += OnActivatingFromFollower,
                        (entity) => entity.OnActivating -= OnActivatingFromFollower);
                }
            }
        }

        void OnActivatingFromFollower(IAWFollower follower)
        {
            Debug.Log($"ALHitBoxBehaviour OnFollowerActiveChanged: {follower}");

            ALColliderExtraInfo extraInfo = follower.GameObject.GetComponent<ALColliderExtraInfo>();
            if (extraInfo == null)
            {
                Debug.Assert(false, $"ALHitBoxBehaviour OnFollowerActiveChanged: ALColliderExtraInfo component not found in {follower.GameObject.name}");
                return;
            }

            extraInfo.SerialNumber = GetNextSerialNumber();
            // Debug.Log($"{currentSerialNumber}");

        }
    }
}
#nullable restore