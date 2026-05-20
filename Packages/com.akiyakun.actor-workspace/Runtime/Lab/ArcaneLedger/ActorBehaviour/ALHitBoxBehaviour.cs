#nullable enable
using UnityEngine;
using afl;
using afl.Service.Input;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    public class ALHitBoxInfo : MonoBehaviour
    {
        public IAWActor Actor { get; set; } = null!;

        [SerializeField, Disable] uint serialNumber = 0;
        public uint SerialNumber { get => serialNumber; set => serialNumber = value; }

        public object? UserData { get; set; }
    }

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
            var hitBoxFolderObject = Variables.Get(AWCoreVariableKey.HitBoxFolder).GetGameObject();
            if (hitBoxFolderObject != null)
            {
                var followers = hitBoxFolderObject.GetComponentsInChildren<IAWFollower>(includeInactive: true);
                for (int i = 0; i < followers.Length; i++)
                {
                    var follower = followers[i];

                    var hitBoxInfo = follower.GameObject.AddComponent<ALHitBoxInfo>();
                    hitBoxInfo.Actor = Actor;

                    EventBag.In(follower,
                        (entity) => entity.OnActivating += OnActivatingFromFollower,
                        (entity) => entity.OnActivating -= OnActivatingFromFollower);
                }
            }
        }

        void OnActivatingFromFollower(IAWFollower follower)
        {
            Debug.Log($"ALHitBoxBehaviour OnFollowerActiveChanged: {follower}");

            ALHitBoxInfo hitBoxInfo = follower.GameObject.GetComponent<ALHitBoxInfo>();
            if (hitBoxInfo == null)
            {
                Debug.Assert(false, $"ALHitBoxBehaviour OnFollowerActiveChanged: ALHitBoxInfo component not found in {follower.GameObject.name}");
                return;
            }

            hitBoxInfo.SerialNumber = GetNextSerialNumber();
            // Debug.Log($"{currentSerialNumber}");

        }
    }
}
#nullable restore