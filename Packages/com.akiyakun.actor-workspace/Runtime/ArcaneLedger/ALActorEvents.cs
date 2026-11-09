#nullable enable

namespace ActorWorkspace.ArcaneLedger
{
    // ArcaneLedger のアクターのイベント
    // 命令を送るものは動詞+、受信するものはOn+(AWCoreActorEvents と同じ)
    //
    // 本来static classだが継承して使いたいのでinterfaceになっています
    // ゲーム側では interface ActorEvents : AWCoreActorEvents, ALActorEvents のように両方を継承して使う
    //
    // MEMO: 被弾のリアクション(DamageReaction)は、受けるのが中核の Behaviour なので AWCoreActorEvents にある
    public interface ALActorEvents
    {
        #region StatusEffect
        public static readonly (string, int id) RevokeStatusEffect = ("RevokeStatusEffect", 0);
        public static readonly (string, ApplyStatusEffectParams) RequestStatusEffect = ("RequestStatusEffect", default);
        public static readonly (string, uint enableFlags) OnStatusEffectChanged = ("OnStatusEffectChanged", 0);
        #endregion// StatusEffect

        #region Damage
        // ダメージを受けさせる(受けるアクターに発行する)。DefaultALProcessor.ApplyDamage() が受ける
        public static readonly (string, ALDamageInfo) RequestDamage = ("RequestDamage", default);
        // HP を回復する(値: 回復量。受けるアクターに発行する)。DefaultALProcessor.ApplyHeal() が受ける
        public static readonly (string, float) RequestHeal = ("RequestHeal", 0.0f);

        // ダメージを受けた(受けたアクターに出る)。計算したダメージが 0 のとき(Result.Reason が Zero)も出る
        public static readonly (string, ALDamagedArgs) OnDamaged = ("OnDamaged", default);
        // ダメージを与えた(攻撃したアクターに出る。ALDamageInfo.Attacker があるときだけ)
        public static readonly (string, ALDamagedArgs) OnDealtDamage = ("OnDealtDamage", default);
        // 倒れた(受けたアクターに出る)
        public static readonly (string, ALDamagedArgs) OnDefeated = ("OnDefeated", default);
        // 倒した(攻撃したアクターに出る。ALDamageInfo.Attacker があるときだけ)
        public static readonly (string, ALDamagedArgs) OnKilled = ("OnKilled", default);
        // 回復した(回復したアクターに出る)
        public static readonly (string, ALHealResult) OnHealed = ("OnHealed", default);
        #endregion// Damage
    }
}
#nullable restore
