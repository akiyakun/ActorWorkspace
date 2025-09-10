#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // アクターの表示のルート分
    // 全体をスケールしたり反転したりするよう
    public class AWActorDisplay : MonoBehaviour
    {
        // public GameObject GameObject { get; protected set; } = null!;

        // IAWActor actor = null!;
        // SpineAnimationController spineAnimationController = null!;
        // // public ISkeletonAnimation SkeletonAnimation => skeletonAnimation;

        // // From IAWActorDisplay
        // public virtual async UniTask<int> InitializeAsync(IAWActor actor, SpineAnimationController spineAnimationController, CancellationToken cancellationToken)
        // {
        //     if (actor == null) return GeneralReturnCode.Failed;

        //     this.spineAnimationController = spineAnimationController;
        //     Debug.Assert(spineAnimationController != null);

        //     if (actor.GameObject.transform.Find("ActorDisplay"))
        //     {
        //     }

        //         return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        // }

        // // From IAWActorDisplay
        // public virtual void Restore()
        // {
        // }

        // // From IAWActorDisplay
        // public virtual void DoUpdate(float deltaTime)
        // {
        // }
    }
}
#nullable restore