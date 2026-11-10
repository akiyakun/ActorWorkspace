#nullable enable
namespace ActorWorkspace.ArcaneLedger
{
    // 当たりの結果(IALProcessor.Hit())
    public struct HitResult
    {
        public static readonly HitResult NotHit = new HitResult() { IsHit = false };

        // 当たりとして扱ったか(陣営・無敵などで弾いたときは false)
        public bool IsHit;
        // リアクションの段階(0 はなし。ゲームが決める)。0 より大きければ AWCoreActorEvents.DamageReaction で知らせる
        public int DamageReaction;
        // ダメージの結果(ダメージを与えなかったときは Applied が false・Reason が None)
        public ALDamageResult Damage;
    }
}
#nullable restore
