#nullable enable
using afl;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // 陣営(0〜31)。ゲーム側の enum を (int) にして使う
    // 攻撃した側の陣営(IALCollisionExtraData.Faction)のビットが、受ける側の HostileMask(ArcaneLedgerBehaviour)に立っていれば当たる
    public static class ALFaction
    {
        public const int MaxCount = 32;
        public const uint All = 0xffffffff;

        public static uint ToBit(int faction) => (faction >= 0 && faction < MaxCount) ? (1u << faction) : 0u;

        public static bool CanHit(int attackerFaction, uint targetHostileMask) => (targetHostileMask & ToBit(attackerFaction)) != 0;
    }

    // 当たり(ALHurtBoxBehaviour が確定して、ArcaneLedgerBehaviour.ContactWithHurtBox() → IALProcessor.Hit() に渡す)
    public struct ALHitContact
    {
        // 当たり判定の通知そのもの(Me は受けた側、Other は攻撃の当たり判定)
        public CollisionContactInfo Raw;
        // 攻撃の当たり判定の情報(HitBox・弾)
        // MEMO: 確定したときの中身は、当たったときと違うことがある(攻撃が元に戻った・弾がプールに戻った)。当たったときの値は下の AttackId などを使う
        public IALCollisionExtraData Attacker;
        // 当たったとき(ALHitResolver.Add())の、攻撃の ID・攻撃したアクター・その ALProcessor
        public int AttackId;
        public IAWActor? AttackerActor;
        public IALProcessor? AttackerProcessor;
        // 受けた側の HurtBox の情報(どの HurtBox か分からないとき(ALHurtBoxDetectMode.Main)は null)
        public IALCollisionExtraData? HurtBox;
        // 当たった攻撃の記録のキー(ALHitHistory.GetHitKey。None なら 0)
        public ulong HitKey;
        // 当たった瞬間の、攻撃した側・受けた側の汎用パラメータの写し(ALHitResolver.Add() のとき。無ければ IsValid が false)
        public ALGeneralParamSnapshot AttackerParams;
        public ALGeneralParamSnapshot ReceiverParams;

        // 部位(0 はなし)と部位のダメージ倍率(0 以下は 1 とみなす)
        public int Region => HurtBox?.Region ?? 0;
        public float RegionMultiplier
        {
            get
            {
                float multiplier = HurtBox?.DamageMultiplier ?? 1.0f;
                return multiplier > 0.0f ? multiplier : 1.0f;
            }
        }
    }
}
#nullable restore
