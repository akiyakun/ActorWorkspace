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
    }
}
#nullable restore
