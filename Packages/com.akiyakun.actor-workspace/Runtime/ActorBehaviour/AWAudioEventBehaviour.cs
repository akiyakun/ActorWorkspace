#nullable enable
using afl;
using afl.GameAudio;

namespace ActorWorkspace.ActorBehaviour
{
    // モーションのAudioイベント(AWCoreActorEvents.Audio。Int値がSEマスタのId)でSEを鳴らす
    // アクターの位置で鳴らす(afl の SEContext。画面の外なら鳴らさない・画面の左右の位置でパンする)
    // 「モーション中断で止める」SE(SEマスタのStopOnMotionInterrupt)は、鳴らしたモーションが別のモーションに変わったときと、
    // アクターが消えたとき(AWCoreActorEvents.Despawn・StopMotionSE)に止める
    //
    // MEMO: SEContext は、初めて鳴らすときに UniversalContextManager から取る(SEを鳴らさないアクターでは使わないように)
    public class AWAudioEventBehaviour : AWActorBehaviour<IAWActor>
    {
        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Manual;

        SEContext? seContext;
        SEMotionInterruptTracker motionInterruptTracker = null!;

        // 今のモーション(Audioイベントを出したモーション)
        int currentMotionNameHash = 0;

        public override void OnRestore()
        {
            // MEMO: アクターを使い回すときに、前のモーションの溜め音などが鳴り続けないようにする
            StopMotionSE();
            currentMotionNameHash = 0;
        }

        public override void OnAwake()
        {
            motionInterruptTracker = new SEMotionInterruptTracker(
                handle => GetSEContext().IsPlaying(handle),
                handle => GetSEContext().Stop(handle, SEContext.MotionInterruptFadeDuration));

            // Events
            {
                EventBag.In(EventBus,
                    (entity) => entity.Subscribe(AWCoreActorEvents.Audio, OnAudio),
                    (entity) => entity.Unsubscribe(AWCoreActorEvents.Audio, OnAudio));
                EventBag.In(EventBus,
                    (entity) => entity.Subscribe(AWCoreActorEvents.Despawn, StopMotionSE),
                    (entity) => entity.Unsubscribe(AWCoreActorEvents.Despawn, StopMotionSE));
                EventBag.In(EventBus,
                    (entity) => entity.Subscribe(AWCoreActorEvents.StopMotionSE, StopMotionSE),
                    (entity) => entity.Unsubscribe(AWCoreActorEvents.StopMotionSE, StopMotionSE));
                EventBag.In(Actor.AnimationController,
                    (entity) => entity.OnAnimationEntered += OnAnimationEntered,
                    (entity) => entity.OnAnimationEntered -= OnAnimationEntered);
            }
        }

        public override void OnDestroy()
        {
            StopMotionSE();
        }

        void StopMotionSE()
        {
            motionInterruptTracker?.StopAll();
        }

        void OnAnimationEntered(IAWAnimation animation)
        {
            currentMotionNameHash = animation.NameHash;
            motionInterruptTracker.OnMotionEntered(animation.NameHash);
        }

        protected virtual void OnAudio(int id)
        {
            var se = GetSEContext();
            var handle = se.Play(id, Actor.GameObject.transform.position);
            if (handle.IsValid && se.IsStopOnMotionInterrupt(id))
            {
                int motionNameHash = currentMotionNameHash != 0
                    ? currentMotionNameHash
                    : Actor.AnimationController.GetCurrentAnimation()?.NameHash ?? 0;
                motionInterruptTracker.Add(handle, motionNameHash);
            }
        }

        SEContext GetSEContext()
        {
            return seContext ??= UniversalContextManager.Instance.GetContext<SEContext>();
        }
    }
}
#nullable restore
