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
    public abstract class AWActorBase<TActorContextProvider, TActorParam, TAnimationController, TSkin>
        : MonoBehaviour, IAWActor
        where TActorContextProvider : AWActorContextProvider
        where TActorParam : class, IAWActorParam
        // where TActorDisplay : class, IAWActorDisplay
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
        // IAWActorDisplay IAWActor.ActorDisplay => ActorDisplay as IAWActorDisplay;
        // public abstract TActorDisplay ActorDisplay { get; protected set; }
        [SerializeField] AWActorDisplay actorDisplay = null!;
        public AWActorDisplay ActorDisplay => actorDisplay;
        IAWAnimationController IAWActor.AnimationController => AnimationController as IAWAnimationController;
        public abstract TAnimationController AnimationController { get; protected set; }
        IReadOnlyList<IAWSkin> IAWActor.SkinList => SkinList as IReadOnlyList<IAWSkin>;
        public abstract IReadOnlyList<TSkin> SkinList { get; }
        public virtual AWActorBehaviourController ActorBehaviourController { get; private set; }

        #region IUpdateElement
        public bool ElementActive { get; set; }
        public int ElementPriority { get; set; } = 0;
        public UpdateFlags UpdateFlags { get; set; } = UpdateFlags.All;
        #endregion

#if UNITY_EDITOR
        // デバッグ確認用
        [Disable] public List<string> debugActorBehaviours = new();
#endif

#nullable disable
        protected AWActorBase() { }
#nullable enable

        public async UniTask<int> InitializeAsync(
            AWActorContextProvider awActorContextProvider, int id, int category, CancellationToken cancellationToken)
        {
            ActorId = id;
            Debug.Assert(ActorId > 0);
            ActorCategory = category;
            Debug.Assert(ActorCategory >= 0);

            Debug.Assert(actorDisplay != null, "ActorDisplay is not set in inspector");

            // ActorContextProvider = awActorContextProvider;
            // Debug.Assert(awActorContextProvider != null);

            ActorBehaviourController = new AWActorBehaviourController(this);

#if UNITY_EDITOR
            // デバッグ用のイベント登録
            ActorBehaviourController.OnBehaviourAdded += (behaviour) =>
            {
                debugActorBehaviours.Add(behaviour.GetType().Name);
            };
#endif

            if (await InnerInitializeAsync(awActorContextProvider, cancellationToken) is int ret && ret < 0) return ret;
            Restore();

            return GeneralReturnCode.Succeeded;
        }

        protected virtual async UniTask<int> InnerInitializeAsync(AWActorContextProvider awActorContextProvider, CancellationToken cancellationToken)
        {
            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        // public void Terminate()
        public virtual void Dispose()
        {
            ActorBehaviourController?.Dispose();
            ActorBehaviourController = null!;

            AnimationController?.Dispose();
            AnimationController = null!;

            // InnerTerminate();
        }

        public virtual void Restore()
        {
            ActorParam?.Restore();
            AnimationController?.Restore();
            ActorBehaviourController?.Restore();
        }

        // From IUpdateElement
        public virtual void DoUpdate(float deltaTime)
        {
            ActorBehaviourController.DoUpdate(deltaTime);
            AnimationController.DoUpdate(deltaTime);
        }

        // From IUpdateElement
        public virtual void DoLateUpdate(float deltaTime)
        {
            ActorBehaviourController.DoLateUpdate(deltaTime);
        }

        // From IUpdateElement
        public virtual void DoFixedUpdate()
        {
            ActorBehaviourController.DoFixedUpdate();
        }

        public virtual void SetSkin(int skinIndex)
        {
        }
    }
}
#nullable restore