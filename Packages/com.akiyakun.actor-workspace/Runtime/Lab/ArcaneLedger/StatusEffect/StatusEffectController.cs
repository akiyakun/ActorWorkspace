#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace.ArcaneLedger
{
    // MEMO;
    // Idがそのままプライオリティとなる
    public class StatusEffectController
    {
        public const int MaxStatusEffectCount = 32;
        // public const int MaxStatusEffectParamCount = 6;
        public const int MaxGeneralParamCount = 3;

        public uint EnableFlag { get; private set; }

        IAWActor actor;
        public IAWActor Actor => actor;

        IALProcessor processor;
        // StatusEffectData[] statusEffects = new StatusEffectData[MaxStatusEffectCount];
        IStatusEffect[] statusEffects = new IStatusEffect[MaxStatusEffectCount];

        uint requestFlag = 0;
        ApplyStatusEffectParams[] requestParams = new ApplyStatusEffectParams[MaxStatusEffectCount];
        // Stack<uint> requestStack = new Stack<uint>(8);

        public StatusEffectController(IAWActor actor, IALProcessor processor)
        {
            this.actor = actor;
            this.processor = processor;

            // for (int id = 0; id < MaxStatusEffectCount; id++)
            // {
            //     statusEffects[id] = new StatusEffectData(id);
            // }
            processor.SetupStatusEffects(statusEffects);
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
            for (int id = 0; id < MaxStatusEffectCount; id++)
            {
                statusEffects[id]?.DoUpdate(deltaTime);
            }
        }

        public void SetStatusEffect(IStatusEffect effect)
        {
            Debug.Assert(effect! != null, "Effect is null");
            Debug.Assert(effect!.Id >= 0 && effect.Id < MaxStatusEffectCount, "Invalid status effect ID");
            effect.Setup(this);
            statusEffects[effect.Id] = effect;
        }

        public void Request(int id, ApplyStatusEffectParams param)
        {
            Debug.Assert(id >= 0 && id < MaxStatusEffectCount, "Invalid status effect ID");
            requestFlag |= (1u << id);
            requestParams[id] = param;
        }

    }
}
#nullable restore