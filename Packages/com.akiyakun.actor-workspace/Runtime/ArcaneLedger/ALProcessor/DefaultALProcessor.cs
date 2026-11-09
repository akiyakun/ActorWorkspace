#nullable enable
using System.Runtime.CompilerServices;
using UnityEngine;
using afl;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;

namespace ActorWorkspace.ArcaneLedger
{
    public class DefaultALProcessor : IALProcessor
    {
        protected ArcaneLedgerBehaviour owner = null!;
        protected IAWActor actor = null!;
        protected StatusEffectController statusEffectController = null!;

        protected ALGeneralParam generalParam = null!;
        protected ALStatTable stats = null!;
        protected ALHealth health = null!;

        protected EventBag eventBag = new();

        // ステータスの表の大きさ・最大 HP のステータスの ID(ゲーム側が override する)
        public virtual int StatCapacity => 0;
        public virtual int MaxLifeStatId => -1;

        public virtual void Setup(ArcaneLedgerBehaviour owner, StatusEffectController statusEffectController)
        {
            if (this.owner != null) throw new System.InvalidOperationException("Already setup");
            this.owner = owner;
            actor = owner.Actor;

            this.statusEffectController = statusEffectController;

            generalParam = owner.GeneralParam;
            stats = owner.Stats;
            health = owner.Health;

            Awake();
        }

        protected virtual void Awake()
        {
            // Events
            {
                eventBag.In(actor.EventBus,
                    (entity) => entity.Subscribe(ALActorEvents.RevokeStatusEffect, RevokeStatusEffect),
                    (entity) => entity.Unsubscribe(ALActorEvents.RevokeStatusEffect, RevokeStatusEffect));
                eventBag.In(actor.EventBus,
                    (entity) => entity.Subscribe(ALActorEvents.RequestStatusEffect, RequestStatusEffect),
                    (entity) => entity.Unsubscribe(ALActorEvents.RequestStatusEffect, RequestStatusEffect));
                eventBag.In(actor.EventBus,
                    (entity) => entity.Subscribe(ALActorEvents.RequestDamage, OnRequestDamage),
                    (entity) => entity.Unsubscribe(ALActorEvents.RequestDamage, OnRequestDamage));
                eventBag.In(actor.EventBus,
                    (entity) => entity.Subscribe(ALActorEvents.RequestHeal, OnRequestHeal),
                    (entity) => entity.Unsubscribe(ALActorEvents.RequestHeal, OnRequestHeal));
            }
        }

        public virtual void Restore()
        {
        }

        public virtual void Prepare()
        {
        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual void FixedUpdate(float deltaTime)
        {
        }


        public virtual void OnStatusEffectChanged(uint enableFlags)
        {
        }


        public virtual HitResult Hit(CollisionContactInfo contactInfo)
        {
            return HitResult.NotHit;
        }


        // デフォルトの汎用パラメータ計算
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Calc(int paramId)
        {
            var param = generalParam[paramId];
            return (param.Value + param.Delta) * param.Factor;
        }

        // protected virtual float CalcDamageReaction()
        // {
        //     var param = GetGeneralParam(GeneralParamType.DamageReaction);
        //     return param.Value + param.Modifier;
        // }

        // protected virtual float CalcDamageReactionResist()
        // {
        //     var param = GetGeneralParam(GeneralParamType.DamageReactionResist);
        //     return param.Value + param.Modifier;
        // }

        // public void SetupStatusEffects(StatusEffect[] statusEffects)
        // {
        //     if (statusEffects == null) throw new System.ArgumentNullException(nameof(statusEffects));

        //     //  statusEffects[(int)StatusEffectId.Freeze] = new StatusEffectFreeze();
        // }

        protected virtual void RevokeStatusEffect(int id)
        {
            statusEffectController.Revoke(id);
        }

        protected virtual void RequestStatusEffect(ApplyStatusEffectParams param)
        {
            statusEffectController.Request(param);
        }


        #region Damage
        void OnRequestDamage(ALDamageInfo info) => ApplyDamage(info);
        void OnRequestHeal(float amount) => ApplyHeal(amount);

        // ダメージを受ける。順番:
        // 1. 倒れていたら受けない(NotAlive)
        // 2. 攻撃(Hit)は、無敵の間は受けない(Invincible)
        // 3. FilterDamage() が false なら受けない(Filtered)
        // 4. CalcDamage() でダメージを計算し、ModifyDamage() で調整する
        // 5. 攻撃(Hit)なら GetInvincibleTimeOnHit() の時間だけ無敵にする(ダメージが 0 でも)
        // 6. ダメージが 0 以下なら HP を減らさない(Zero。OnDamaged は出す)
        // 7. HP を減らし、OnDamaged・OnDealtDamage を出す。HP が 0 になったら倒れて OnDefeated・OnKilled を出す
        // 知らせる順番は、virtual(OnDamaged()・OnDefeated())が先で、イベントがあと
        public virtual ALDamageResult ApplyDamage(ALDamageInfo info)
        {
            float life = health.Current;
            if (health.IsAlive == false) return ALDamageResult.Rejected(ALDamageRejectReason.NotAlive, life);
            if (info.Kind == ALDamageKind.Hit && health.IsInvincible) return ALDamageResult.Rejected(ALDamageRejectReason.Invincible, life);
            if (FilterDamage(ref info) == false) return ALDamageResult.Rejected(ALDamageRejectReason.Filtered, life);

            info.Value = CalcDamage(in info);
            ModifyDamage(ref info);

            if (info.Kind == ALDamageKind.Hit)
            {
                float invincibleTime = GetInvincibleTimeOnHit(in info);
                if (invincibleTime > health.InvincibleTime) health.InvincibleTime = invincibleTime;
            }

            if ((info.Value > 0.0f) == false)
            {
                var zero = ALDamageResult.Rejected(ALDamageRejectReason.Zero, life);
                NotifyDamaged(new ALDamagedArgs(actor, info, zero));
                return zero;
            }

            health.Current = Mathf.Max(0.0f, life - info.Value);
            var result = new ALDamageResult()
            {
                Applied = true,
                Reason = ALDamageRejectReason.None,
                Damage = info.Value,
                LifeBefore = life,
                LifeAfter = health.Current,
                Killed = health.Current <= 0.0f,
            };
            if (result.Killed) health.IsAlive = false;

            var args = new ALDamagedArgs(actor, info, result);
            NotifyDamaged(args);
            if (result.Killed)
            {
                OnDefeated(in args);
                actor.EventBus.Publish(ALActorEvents.OnDefeated, args);
                info.Attacker?.EventBus.Publish(ALActorEvents.OnKilled, args);
            }
            return result;
        }

        void NotifyDamaged(in ALDamagedArgs args)
        {
            OnDamaged(in args);
            actor.EventBus.Publish(ALActorEvents.OnDamaged, args);
            args.Info.Attacker?.EventBus.Publish(ALActorEvents.OnDealtDamage, args);
        }

        // HP を回復する(最大 HP まで)。倒れていたら回復しない
        public virtual ALHealResult ApplyHeal(float amount)
        {
            if (health.IsAlive == false || (amount > 0.0f) == false) return new ALHealResult() { LifeAfter = health.Current };
            if (FilterHeal(ref amount) == false) return new ALHealResult() { LifeAfter = health.Current };

            float before = health.Current;
            health.Current = Mathf.Min(health.Max, before + amount);
            var result = new ALHealResult() { Applied = true, Amount = health.Current - before, LifeAfter = health.Current };

            OnHealed(in result);
            actor.EventBus.Publish(ALActorEvents.OnHealed, result);
            return result;
        }

        // ゲーム側が override するところ
        // ダメージを受けるか(false なら受けない。陣営・カテゴリなど)
        protected virtual bool FilterDamage(ref ALDamageInfo info) => true;
        // ダメージの計算式。既定は ALDamageInfo.Value のまま
        protected virtual float CalcDamage(in ALDamageInfo info) => info.Value;
        // 計算したあとの調整(HP が 1 より下がらない など)
        protected virtual void ModifyDamage(ref ALDamageInfo info) { }
        // 攻撃(Hit)を受けたときの無敵時間(秒)。0 なら無敵にならない
        protected virtual float GetInvincibleTimeOnHit(in ALDamageInfo info) => 0.0f;
        // ダメージを受けた(計算したダメージが 0 のときも呼ばれる。args.Result.Reason が Zero)
        protected virtual void OnDamaged(in ALDamagedArgs args) { }
        // 倒れた
        protected virtual void OnDefeated(in ALDamagedArgs args) { }
        // 回復するか(false なら回復しない)。amount を変えてもよい
        protected virtual bool FilterHeal(ref float amount) => true;
        // 回復した
        protected virtual void OnHealed(in ALHealResult result) { }
        #endregion

    }
}
#nullable restore