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

        // MEMO:ビットが1のとき無効になるマスク
        public uint RequestMask { get; set; }
        // RequestMaskでマスクを許可するビット
        public uint RequestUnmask { get; set; } = 0xffffffff;
        public uint RequestFlags { get; private set; }
        ApplyStatusEffectParams[] requestParams = new ApplyStatusEffectParams[MaxStatusEffectCount];

        // EventBag eventBag = new();

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

            // Events
            // {
            //     eventBag.In(Actor.EventBus,
            //         (entity) => entity.Subscribe(AWCoreActorEvents.RequestStatusEffect, RequestStatusEffectChanged),
            //         (entity) => entity.Unsubscribe(AWCoreActorEvents.RequestStatusEffect, RequestStatusEffectChanged));
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
            // RequestMask = 0xffffffff;
            RequestMask = 0;

            for (int id = 0; id < MaxStatusEffectCount; id++)
            {
                statusEffects[id]?.Prepare();
            }
        }

        public void Update(float deltaTime)
        {
            // EnableFlags = 0;

            // マスクを考慮した有効なリクエストフラグ
            uint requestBit = ~(RequestMask & RequestUnmask) & RequestFlags;
            // ~(1110 & 0111 = 0110) -> 1001
            // 1001 & 0010 -> 0000

            for (int id = 0; id < MaxStatusEffectCount; id++)
            {
                var statusEffect = statusEffects[id];
                if (statusEffect == null) continue;

                // リクエストの処理
                if ((requestBit & (1u << id)) != 0)
                {
                    RequestFlags &= ~(1u << id);
                    statusEffect.Apply(requestParams[id]);
                }

                if (statusEffect.Enable == false) continue;
                statusEffect.Update(deltaTime);
                // if (statusEffect.Enable > 0) EnableFlags |= (1u << id);
            }
        }

        // ステータス効果クラスを取得
        public StatusEffect? GetStatusEffect(int id)
        {
            Debug.Assert(id >= 0 && id < MaxStatusEffectCount, "Invalid status effect ID");
            return statusEffects[id];
        }

        // ステータス効果クラスを設定
        public void SetStatusEffect(StatusEffect statusEffect)
        {
            Debug.Assert(statusEffect! != null, "Effect is null");
            Debug.Assert(statusEffect!.Id >= 0 && statusEffect.Id < MaxStatusEffectCount, "Invalid status effect ID");
            statusEffect.Setup(this);
            statusEffects[statusEffect.Id] = statusEffect;
            statusEffect.OnEnableChanged += OnEnableChanged;
        }

        // Dispel
        public void Revoke(int id)
        {
            Debug.Assert(id >= 0 && id < MaxStatusEffectCount, "Invalid status effect ID");

            statusEffects[id]?.Restore();

        }

        public void Request(ApplyStatusEffectParams applyParam)
        {
            int id = applyParam.Id;
            Debug.Assert(id >= 0 && id < MaxStatusEffectCount, "Invalid status effect ID");

            // 成功判定
            if (RandomEx.Chance(applyParam.ApplyChance) == false) return;

            // if (id == 1)
            // {
            //     D.DebugBreak();
            // }

            // マスクされている場合はリクエストを詰む
            if (((RequestMask & RequestUnmask) & (1u << id)) != 0)
            {
                // (1110 & 0111 = 0110) & (0010) = 0010
                RequestFlags |= (1u << id);
                requestParams[id] = applyParam;
            }
            else
            {
                // 直接適用
                statusEffects[id]?.Apply(applyParam);
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

            // 通知
            owner.Processor.OnStatusEffectChanged(EnableFlags);
            owner.Actor.EventBus.Publish(AWCoreActorEvents.OnStatusEffectChanged, EnableFlags);
        }

    }
}
#nullable restore