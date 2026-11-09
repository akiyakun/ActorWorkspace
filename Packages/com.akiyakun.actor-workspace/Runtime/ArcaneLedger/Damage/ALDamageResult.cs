#nullable enable

namespace ActorWorkspace.ArcaneLedger
{
    // ダメージを受けなかった理由
    // MEMO: ゲーム側で数値として保存することがあるので、末尾に追加すること
    public enum ALDamageRejectReason
    {
        None = 0,           // 受けた
        NotAlive = 1,       // もう倒れている
        Invincible = 2,     // 無敵の間(攻撃のダメージだけ)
        Filtered = 3,       // ゲームが受けないと決めた(DefaultALProcessor.FilterDamage())
        Zero = 4,           // 計算したダメージが 0 以下(当たったことは OnDamaged で知らせる)
    }

    // ダメージの結果
    public struct ALDamageResult
    {
        // HP を減らしたか
        public bool Applied;
        public ALDamageRejectReason Reason;
        // 与えたダメージ(HP より大きくても、そのままの値。ダメージの数字に使う)。受けなかったときは 0
        public float Damage;
        public float LifeBefore;
        public float LifeAfter;
        // このダメージで倒れたか
        public bool Killed;

        public static ALDamageResult Rejected(ALDamageRejectReason reason, float life)
        {
            return new ALDamageResult() { Applied = false, Reason = reason, LifeBefore = life, LifeAfter = life };
        }
    }

    // 回復の結果
    public struct ALHealResult
    {
        public bool Applied;
        // 増えた HP(最大 HP で切ったあとの値)
        public float Amount;
        public float LifeAfter;
    }

    // ダメージの知らせ(ALActorEvents.OnDamaged・OnDealtDamage・OnDefeated・OnKilled)
    public readonly struct ALDamagedArgs
    {
        // ダメージを受けたアクター
        public readonly IAWActor Target;
        public readonly ALDamageInfo Info;
        public readonly ALDamageResult Result;

        public ALDamagedArgs(IAWActor target, in ALDamageInfo info, in ALDamageResult result)
        {
            Target = target;
            Info = info;
            Result = result;
        }
    }
}
#nullable restore
