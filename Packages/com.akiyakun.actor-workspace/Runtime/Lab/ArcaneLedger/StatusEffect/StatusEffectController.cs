#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;

namespace ActorWorkspace.ArcaneLedger
{
    // MEMO;
    // Idがそのままプライオリティとなる
    public class StatusEffectController
    {
        public const int MaxStatusEffectCount = 32;
        // public const int MaxStatusEffectParamCount = 6;
        public const int MaxGeneralParamCount = 3;

        public uint EnableFlags { get; private set; }

        ArcaneLedgerBehaviour owner;
        // IAWActor actor;
        public IAWActor Actor { get; private set; } = null!;

        // IALProcessor processor;
        StatusEffect[] statusEffects = new StatusEffect[MaxStatusEffectCount];

        uint requestFlags = 0;
        ApplyStatusEffectParams[] requestParams = new ApplyStatusEffectParams[MaxStatusEffectCount];

        public StatusEffectController(ArcaneLedgerBehaviour owner)
        {
            this.owner = owner;
            // this.processor = processor;
            this.Actor = owner.Actor;

            // for (int id = 0; id < MaxStatusEffectCount; id++)
            // {
            //     statusEffects[id] = new StatusEffectData(id);
            // }
            // processor.SetupStatusEffects(statusEffects);
        }

        public void Restore()
        {
            for (int id = 0; id < MaxStatusEffectCount; id++)
            {
                statusEffects[id]?.Restore();
            }
        }

        public void DoPrepare()
        {
            for (int id = 0; id < MaxStatusEffectCount; id++)
            {
                statusEffects[id]?.DoPrepare();
            }
        }

        public void DoUpdate(float deltaTime)
        {
            EnableFlags = 0;

            for (int id = 0; id < MaxStatusEffectCount; id++)
            {
                var statusEffect = statusEffects[id];
                if (statusEffect == null) continue;
                statusEffect.DoUpdate(deltaTime);
                if (statusEffect.Enable > 0) EnableFlags |= (1u << id);
            }
        }

        public void SetStatusEffect(StatusEffect statusEffect)
        {
            Debug.Assert(statusEffect! != null, "Effect is null");
            Debug.Assert(statusEffect!.Id >= 0 && statusEffect.Id < MaxStatusEffectCount, "Invalid status effect ID");
            statusEffect.Setup(this);
            statusEffects[statusEffect.Id] = statusEffect;
        }

        public void Request(int id, ApplyStatusEffectParams param)
        {
            Debug.Assert(id >= 0 && id < MaxStatusEffectCount, "Invalid status effect ID");

            if (RandomEx.Chance(param.ApplyChance) == false) return;

            requestFlags|= (1u << id);
            requestParams[id] = param;

            // FIXME: 仮
            if (statusEffects[id] != null)
            {
                statusEffects[id].Apply(param);
            }
        }

    }
}
#nullable restore