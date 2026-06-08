#nullable enable
using UnityEngine;
using afl;
using ActorWorkspace;

namespace Project.ActorBehaviour
{
    // デフォルト実装のモーションの状態管理クラス
    public abstract class AWDefaultActorMotionStateBehaviour<TActor> : AWActorBehaviour<TActor>
        where TActor : class, IAWActor
    {
        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Update;

        float stateElapsedTime;

        enum ResetDirtyFlags
        {
            DamageReaction = 1 << 0,
        }
        uint resetDirtyFlag = 0;

        public override void Restore()
        {
            ResetAnimationParams();
            // Actor.AnimationController.SetAnimation(ActorMotionNames.Idle, loop: true, immediate: true);
            base.Restore();
        }

        public override void Awake()
        {
            // イベントの購読
            {
                // AnimationController
                EventBag.In(Actor.AnimationController,
                    (entity) => entity.OnAnimationEntered += OnAnimationEntered,
                    (entity) => entity.OnAnimationEntered -= OnAnimationEntered);
                EventBag.In(Actor.AnimationController,
                    (entity) => entity.OnAnimationComplete += OnAnimationComplete,
                    (entity) => entity.OnAnimationComplete -= OnAnimationComplete);

                // AWCoreActorEvents
                EventBag.In(Actor.EventBus,
                    (entity) => entity.Subscribe(AWCoreActorEvents.DamageReaction, DamageReaction),
                    (entity) => entity.Unsubscribe(AWCoreActorEvents.DamageReaction, DamageReaction));

                // var worldEventContext = UniversalContextManager.Instance.GetContext<WorldEventContext>();
                // EventBag.In(worldEventContext,
                //     (entity) => entity.Subscribe(WorldEvents.CurrentLevelSceneChanged, Restore),
                //     (entity) => entity.Unsubscribe(WorldEvents.CurrentLevelSceneChanged, Restore));

                // EventBag.In(Actor.StateMachineBehaviour,
                //     (entity) => entity.AddCallback(DefaultActorState.Prepare, StateMachineCallbackOrder.Enter, OnEnterStateOfPrepare),
                //     (entity) => entity.RemoveCallback(DefaultActorState.Prepare, StateMachineCallbackOrder.Enter, OnEnterStateOfPrepare));

                // EventBag.In(EventBus,
                //     (entity) => entity.Subscribe(ActorEvents.OnFall, OnFall),
                //     (entity) => entity.Unsubscribe(ActorEvents.OnFall, OnFall));

            }

            base.Awake();
        }

        public override void DoUpdate(float deltaTime)
        {
            stateElapsedTime += deltaTime;
            // Actor.AnimationController.AnimationParameter.SetFloat(ActorMotionParams.StateElapsedTime, stateElapsedTime);
            base.DoUpdate(deltaTime);
        }

        // アニメーションパラメーターのリセット
        protected virtual void ResetAnimationParams()
        {
            Actor.AnimationController.AnimationParameter.AllResetTrigger();

            {
                var dirtyFlag = resetDirtyFlag;
                resetDirtyFlag = 0;

                if ((dirtyFlag & (1u << (int)ResetDirtyFlags.DamageReaction)) != 0)
                {
                    Actor.AnimationController.AnimationParameter.SetInt(AWCoreActorMotionParams.DamageReaction, 0);
                }
            }
        }

        // MEMO: アニメーションの1Frame処理後に発火するっぽい？
        protected virtual void OnAnimationEntered(IAWAnimation animation)
        {
            ResetAnimationParams();
            stateElapsedTime = 0.0f;
        }

        protected virtual void OnAnimationComplete(IAWAnimation animation)
        {
            stateElapsedTime = 0.0f;
        }

        // protected virtual void OnEnterStateOfPrepare(StateCallbackInfo info)
        // {
        //     ResetAnimationParams();
        // }

        #region AWCoreActorEvents
        protected virtual void DamageReaction(int value)
        {
            Actor.AnimationController.AnimationParameter.SetInt(AWCoreActorMotionParams.DamageReaction, value);
            resetDirtyFlag |= (uint)(1u << (int)ResetDirtyFlags.DamageReaction);
        }
        #endregion


        // void OnGround(bool isGrounded)
        // {
        //     ResetAnimationParams();
        //     Actor.AnimationController.AnimationParameter.SetBool(ActorMotionParams.IsGrounded, isGrounded);
        // }

        // protected virtual void OnFall()
        // {
        //     ResetAnimationParams();
        //     // Actor.AnimationController.SetAnimation(ActorMotionNames.Fall, loop: true, immediate: true);
        //     // Actor.AnimationController.SetAnimation(ActorMotionNames.Idle, loop: true, immediate: true);
        //     Actor.AnimationController.AnimationParameter.SetTrigger(ActorMotionNames.Fall);
        // }
    }
}
#nullable restore