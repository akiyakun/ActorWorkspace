#nullable enable
using UnityEngine;

namespace ActorWorkspace.ArcaneLedger
{
    // アクターの HP・生死・無敵時間
    // 最大 HP は ALStatTable の最終値(IALProcessor.Config.MaxLifeStatId)。-1 なら HP を持たない(Max は 0)
    //
    // ダメージ・回復は DefaultALProcessor.ApplyDamage()・ApplyHeal() を通す(無敵・撃破の判定と通知をそこで行う)
    // MEMO: プロパティを直接書き換えると、判定も通知も通らない(テスト・演出・ダメージによらない退場などに使う)
    public sealed class ALHealth
    {
        readonly ALStatTable stats;
        readonly int maxLifeStatId;

        public ALHealth(ALStatTable stats, int maxLifeStatId)
        {
            this.stats = stats;
            this.maxLifeStatId = maxLifeStatId;

            // 最大 HP が下がったら、今の HP も最大 HP までにする(上がったときは今の HP のまま)
            if (maxLifeStatId >= 0) stats.OnChanged += OnStatChanged;
        }

        public bool HasLife => maxLifeStatId >= 0;

        // 最大 HP
        public float Max => maxLifeStatId >= 0 ? stats.Get(maxLifeStatId) : 0.0f;

        // 今の HP
        public float Current { get; set; }

        public bool IsAlive { get; set; } = true;

        // 攻撃(ALDamageKind.Hit)のダメージを受けない残りの時間(秒)
        public float InvincibleTime { get; set; }
        public bool IsInvincible => InvincibleTime > 0.0f;

        // 初期状態に戻す(HP を最大にする、生きている、無敵でない)
        public void Reset()
        {
            Current = Max;
            IsAlive = true;
            InvincibleTime = 0.0f;
        }

        // 無敵時間を減らす(ArcaneLedgerBehaviour の Update から呼ぶ)
        public void Update(float deltaTime)
        {
            if (InvincibleTime > 0.0f) InvincibleTime = Mathf.Max(0.0f, InvincibleTime - deltaTime);
        }

        void OnStatChanged(int id)
        {
            if (id != maxLifeStatId) return;
            float max = Max;
            if (Current > max) Current = max;
        }
    }
}
#nullable restore
