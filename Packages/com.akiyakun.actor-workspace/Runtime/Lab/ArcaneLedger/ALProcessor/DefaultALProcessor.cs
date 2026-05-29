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

        public virtual void Setup(ArcaneLedgerBehaviour owner, StatusEffectController statusEffectController)
        {
            if (this.owner != null) throw new System.InvalidOperationException("Already setup");
            this.owner = owner;
            actor = owner.Actor;

            this.statusEffectController = statusEffectController;

            generalParam = owner.GeneralParam;

            Awake();
        }

        protected virtual void Awake()
        {
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

    }
}
#nullable restore