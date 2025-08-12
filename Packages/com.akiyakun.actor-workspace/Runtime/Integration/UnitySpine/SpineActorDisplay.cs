using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Spine.Unity;
using afl;

namespace ActorWorkspace.UnitySpine
{
    public class SpineActorDisplay : IAWActorDisplay
    {
        ISkeletonAnimation skeletonAnimation;
        public ISkeletonAnimation SkeletonAnimation => skeletonAnimation;

        // From IAWActorDisplay
        public virtual async UniTask<int> InitializeAsync(SkeletonAnimation skeletonAnimation, CancellationToken cancellationToken)
        {
            this.skeletonAnimation = skeletonAnimation;
            Debug.Assert(skeletonAnimation != null);

            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        // From IAWActorDisplay
        public virtual void Restore()
        {

        }

        // From IAWActorDisplay
        public virtual void DoUpdate(float deltaTime)
        {

        }
    }
}
