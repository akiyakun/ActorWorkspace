#nullable enable
using UnityEngine;
using afl;
using afl.Service.Input;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    public class ALHitBoxBehaviour : AWActorBehaviour<IAWActor>
    {

        public override void Initialize(IAWActor actor)
        {
            base.Initialize(actor);

            var hitBoxFolderObject = Variables.Get(AWCoreVariableKey.HitBoxFolder).GetGameObject();
            if (hitBoxFolderObject != null)
            {
                var followers = hitBoxFolderObject.GetComponentsInChildren<IAWFollower>(includeInactive: true);
                for (int i = 0; i < followers.Length; i++)
                {
                    var follower = followers[i];

                    EventBag.In(follower,
                        (entity) => entity.OnActiveChange += OnFollowerActiveChanged,
                        (entity) => entity.OnActiveChange -= OnFollowerActiveChanged);
                }
            }
        }

        public override void Restore()
        {

        }

        void OnFollowerActiveChanged(IAWFollower follower, bool isActive)
        {
            Debug.Log($"ALHitBoxBehaviour OnFollowerActiveChanged: {follower}, isActive: {isActive}");
        }
    }
}
#nullable restore