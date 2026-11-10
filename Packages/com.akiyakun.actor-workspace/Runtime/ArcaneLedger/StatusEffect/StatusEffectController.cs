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
        public ArcaneLedgerBehaviour Owner => owner;
        public ALGeneralParam GeneralParam { get; } = null!;
        public ALStatTable Stats { get; } = null!;
        // 移動速度のステータスの ID(IALProcessor.MoveSpeedStatId)。-1 なら移動速度を変える効果は何もしない
        public int MoveSpeedStatId => owner.Processor.MoveSpeedStatId;

        // IALProcessor processor;
        StatusEffect[] statusEffects = new StatusEffect[MaxStatusEffectCount];

        // MEMO:ビットが1のとき無効になるマスク
        public uint RequestMask { get; set; }
        // RequestMaskでマスクを許可するビット
        public uint RequestUnmask { get; set; } = 0xffffffff;
        public uint RequestFlags { get; private set; }
        ApplyStatusEffectParams[] requestParams = new ApplyStatusEffectParams[MaxStatusEffectCount];

        // 重ね掛けのルールの対象(ビットが 1 の状態異常どうしで StackRule を使う)。0 ならルールを使わない
        public uint ExclusiveMask { get; set; }
        // ExclusiveMask の状態異常に、別の種類の ExclusiveMask の状態異常がかかったときのルール
        // MEMO: 同じ種類をかけ直したときは、どのルールでも Apply() で値と効果時間が新しくなる
        public ALStatusEffectStackRule StackRule { get; set; } = ALStatusEffectStackRule.Stack;

        // EventBag eventBag = new();

        public StatusEffectController(ArcaneLedgerBehaviour owner)
        {
            this.owner = owner;
            // this.processor = processor;
            Actor = owner.Actor;
            GeneralParam = owner.GeneralParam;
            Stats = owner.Stats;

            // for (int id = 0; id < MaxStatusEffectCount; id++)
            // {
            //     statusEffects[id].OnEnableChanged += OnEnableChanged;
            // }

            // Events
            // {
            //     eventBag.In(Actor.EventBus,
            //         (entity) => entity.Subscribe(ALActorEvents.RequestStatusEffect, RequestStatusEffectChanged),
            //         (entity) => entity.Unsubscribe(ALActorEvents.RequestStatusEffect, RequestStatusEffectChanged));
            // }
        }

        public void Restore()
        {
            for (int id = 0; id < MaxStatusEffectCount; id++)
            {
                statusEffects[id]?.Restore();
            }

            // 待たせていたリクエストも捨てる(使い回したアクターに前のリクエストがかからないように)
            RequestFlags = 0;
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

        // 物理の更新。かかっている効果の OnFixedUpdate() を呼ぶ(リクエストの処理・効果時間は Update() で行う)
        public void FixedUpdate(float deltaTime)
        {
            for (int id = 0; id < MaxStatusEffectCount; id++)
            {
                statusEffects[id]?.FixedUpdate(deltaTime);
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
        // かかっている効果を終わらせる。待たせているリクエストも捨てる
        public void Revoke(int id)
        {
            Debug.Assert(id >= 0 && id < MaxStatusEffectCount, "Invalid status effect ID");

            statusEffects[id]?.Restore();
            CancelRequest(id);
        }

        // 待たせているリクエストだけを捨てる(かかっている効果はそのまま)
        public void CancelRequest(int id)
        {
            Debug.Assert(id >= 0 && id < MaxStatusEffectCount, "Invalid status effect ID");

            RequestFlags &= ~(1u << id);
        }

        // かかっている効果のうち、いちばん優先度の高いもの(Id がいちばん小さいもの)の Id。かかっていなければ -1
        public int GetTopPriorityEnabledId()
        {
            if (EnableFlags == 0) return -1;
            return BitUtility.FirstSetBitIndex(EnableFlags);
        }

        // ステータス効果をかける
        // 戻り値: かけたか(待たせたときも true)。成功判定に外れた・重ね掛けのルールでかけなかったときは false
        public bool Request(ApplyStatusEffectParams applyParam)
        {
            int id = applyParam.Id;
            Debug.Assert(id >= 0 && id < MaxStatusEffectCount, "Invalid status effect ID");
            // 登録していない効果はかけない(待たせると、Update が飛ばすのでビットが残り続け、重ね掛けのルールでほかの効果がかからなくなる)
            if (statusEffects[id] == null) return false;
            uint bit = 1u << id;

            // 成功判定。1 以上は必ず成功にする
            // MEMO: RandomEx.Chance() は Random.value < 成功率 で、Random.value は 1.0 を含むので、1.0 でも失敗することがある
            // MEMO: 重ね掛けのルールで今の効果を終わらせる前に判定する(外れたときに今の効果が消えないように)
            if (applyParam.ApplyChance < 1.0f && RandomEx.Chance(applyParam.ApplyChance) == false) return false;

            // 重ね掛けのルール
            if ((ExclusiveMask & bit) != 0)
            {
                // 今かかっている(待たせているものも含む)、ほかの種類の効果
                uint others = (EnableFlags | RequestFlags) & ExclusiveMask & ~bit;
                if (others != 0)
                {
                    switch (StackRule)
                    {
                        case ALStatusEffectStackRule.KeepCurrent:
                            return false;
                        case ALStatusEffectStackRule.Overwrite:
                            for (int otherId = 0; otherId < MaxStatusEffectCount; otherId++)
                            {
                                if ((others & (1u << otherId)) == 0) continue;
                                Revoke(otherId);
                            }
                            break;
                        case ALStatusEffectStackRule.Stack:
                            break;
                        default:
                            Debug.Assert(false, $"StatusEffectController: StackRule is out of range. StackRule={StackRule}");
                            break;
                    }
                }
            }

            // マスクされている場合はリクエストを詰む
            if (((RequestMask & RequestUnmask) & bit) != 0)
            {
                // (1110 & 0111 = 0110) & (0010) = 0010
                RequestFlags |= bit;
                requestParams[id] = applyParam;
            }
            else
            {
                // 直接適用
                statusEffects[id]?.Apply(applyParam);
            }
            return true;
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
            owner.Actor.EventBus.Publish(ALActorEvents.OnStatusEffectChanged, EnableFlags);
        }

    }
}
#nullable restore