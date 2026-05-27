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
        public ALGeneralParam GeneralParam { get; } = null!;

        // IALProcessor processor;
        StatusEffect[] statusEffects = new StatusEffect[MaxStatusEffectCount];

        uint requestFlags = 0;
        ApplyStatusEffectParams[] requestParams = new ApplyStatusEffectParams[MaxStatusEffectCount];

        public StatusEffectController(ArcaneLedgerBehaviour owner)
        {
            this.owner = owner;
            // this.processor = processor;
            Actor = owner.Actor;
            GeneralParam = owner.GeneralParam;

            // for (int id = 0; id < MaxStatusEffectCount; id++)
            // {
            //     statusEffects[id].OnEnableChanged += OnEnableChanged;
            // }
        }

        public void Restore()
        {
            for (int id = 0; id < MaxStatusEffectCount; id++)
            {
                statusEffects[id]?.Restore();
            }
        }

        public void Prepare()
        {
            for (int id = 0; id < MaxStatusEffectCount; id++)
            {
                statusEffects[id]?.Prepare();
            }
        }

        public void Update(float deltaTime)
        {
            // EnableFlags = 0;

            for (int id = 0; id < MaxStatusEffectCount; id++)
            {
                var statusEffect = statusEffects[id];
                if (statusEffect == null || statusEffect.Enable == false) continue;
                statusEffect.Update(deltaTime);
                // if (statusEffect.Enable > 0) EnableFlags |= (1u << id);
            }
        }

        public void SetStatusEffect(StatusEffect statusEffect)
        {
            Debug.Assert(statusEffect! != null, "Effect is null");
            Debug.Assert(statusEffect!.Id >= 0 && statusEffect.Id < MaxStatusEffectCount, "Invalid status effect ID");
            statusEffect.Setup(this);
            statusEffects[statusEffect.Id] = statusEffect;
            statusEffect.OnEnableChanged += OnEnableChanged;
        }

        public void Request(int id, ApplyStatusEffectParams applyParam)
        {
            Debug.Assert(id >= 0 && id < MaxStatusEffectCount, "Invalid status effect ID");

            if (RandomEx.Chance(applyParam.ApplyChance) == false) return;

            requestFlags |= (1u << id);
            requestParams[id] = applyParam;

            // FIXME: 仮
            if (statusEffects[id] != null)
            {
                statusEffects[id].Apply(applyParam);
            }
        }

        void OnEnableChanged(int id, bool enable)
        {
            if (enable)
            {
                EnableFlags |= (1u << id);
            }
            else
            {
                EnableFlags &= ~(1u << id);
            }
        }

    }
}
#nullable restore