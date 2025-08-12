#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // 実装時にこのクラスを継承すると手間が省けます
    // 継承は必須ではありません
    public abstract class AWActorBase<TActorContextProvider, TActorParam, TActorDisplay, TAnimationController, TSkin>
        : MonoBehaviour, IAWActor
        where TActorContextProvider : AWActorContextProvider
        where TActorParam : class, IAWActorParam
        where TActorDisplay : class, IAWActorDisplay
        where TAnimationController : class, IAWAnimationController
        where TSkin : class, IAWSkin
    {
        public int ActorId { get; protected set; }
        public int ActorCategory { get; protected set; }
        public GameObject GameObject => this.gameObject;

        AWActorContextProvider IAWActor.ActorContextProvider => ActorContextProvider as AWActorContextProvider;
        public abstract TActorContextProvider ActorContextProvider { get; protected set; }

        IAWActorParam IAWActor.ActorParam => ActorParam as IAWActorParam;
        public abstract TActorParam ActorParam { get; protected set; }
        IAWActorDisplay IAWActor.ActorDisplay => ActorDisplay as IAWActorDisplay;
        public abstract TActorDisplay ActorDisplay { get; protected set; }
        IAWAnimationController IAWActor.AnimationController => AnimationController as IAWAnimationController;
        public abstract TAnimationController AnimationController { get; protected set; }
        IReadOnlyList<IAWSkin> IAWActor.SkinList => SkinList as IReadOnlyList<IAWSkin>;
        public abstract IReadOnlyList<TSkin> SkinList { get; }
        public virtual IAWActorBehaviourController ActorBehaviourController { get; protected set; } = null!;

        #region IUpdateElement
        public bool ElementActive { get; set; }
        public int ElementPriority { get; set; } = 0;
        public UpdateFlags UpdateFlags { get; set; } = UpdateFlags.Update;
        public virtual void DoUpdate(float deltaTime) { }
        public virtual void DoLateUpdate(float deltaTime) { }
        public virtual void DoFixedUpdate() { }
        #endregion

        // AWActorContextProvider awActorContextProvider;
        // SkeletonAnimation skeletonAnimation;
        // IAWEventDecoder eventDecoder;
        // SpineSkeletonAnimationController spineSkeletonAnimationController;
        // List<SpineSkin> skinList;

        public async UniTask<int> InitializeAsync(
            AWActorContextProvider awActorContextProvider, int id, int category, CancellationToken cancellationToken)
        {
            ActorId = id;
            Debug.Assert(ActorId > 0);
            ActorCategory = category;
            Debug.Assert(ActorCategory >= 0);

            // ActorContextProvider = awActorContextProvider;
            // Debug.Assert(awActorContextProvider != null);

            ActorBehaviourController = new AWActorBehaviourController(this);

            return await InnerInitializeAsync(awActorContextProvider, cancellationToken);
        }

        protected virtual async UniTask<int> InnerInitializeAsync(AWActorContextProvider awActorContextProvider, CancellationToken cancellationToken)
        {
            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        public virtual void Restore()
        {
            ActorParam?.Restore();
        }

        public virtual void SetSkin(int skinIndex)
        {
        }
    }
}
#nullable restore