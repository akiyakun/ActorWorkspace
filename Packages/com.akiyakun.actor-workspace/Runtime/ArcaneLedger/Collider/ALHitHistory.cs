#nullable enable
using System.Collections.Generic;

namespace ActorWorkspace.ArcaneLedger
{
    // 同じ攻撃が同じ相手に何度も当たるのを防ぐ方法(IALCollisionExtraData.HitHistoryMode)
    // MEMO: ゲーム側で数値として保存することがあるので、末尾に追加すること
    public enum ALHitHistoryMode
    {
        None = 0,           // 防がない(当たるたびに処理する)
        PerActivation = 1,  // SerialNumber ごと(HitBox は出るたびに番号が変わるので、出ている間に 1 回)
        PerSource = 2,      // 当たり判定(ExtraData)ごと。RepeatHitInterval の間隔で当たり直す(当たっても消えない粒・置く弾・範囲など)
        PerParticle = 3,    // 粒ごと【未実装】今は PerSource と同じ
    }

    // 受ける側(ALHurtBoxBehaviour)が持つ、当たった攻撃の記録
    // キー(ALHitContact.HitKey)ごとに、最後に当たった時刻を覚える
    public sealed class ALHitHistory
    {
        // この時間より前の記録は、Prune() で捨てる
        public const float DefaultKeepTime = 30.0f;

        readonly Dictionary<ulong, float> lastHitTimes = new();
        readonly List<ulong> removeKeys = new();

        public int Count => lastHitTimes.Count;

        // 当たってよいか。repeatHitInterval が 0 以下なら、一度当たったキーは当たらない
        public bool CanHit(ulong hitKey, float repeatHitInterval, float now)
        {
            if (lastHitTimes.TryGetValue(hitKey, out float last) == false) return true;
            if (repeatHitInterval <= 0.0f) return false;
            return now - last >= repeatHitInterval;
        }

        public void Mark(ulong hitKey, float now)
        {
            lastHitTimes[hitKey] = now;
        }

        public void Clear()
        {
            lastHitTimes.Clear();
        }

        // 古い記録を捨てる(keepTime より前に当たったもの)
        public void Prune(float now, float keepTime = DefaultKeepTime)
        {
            removeKeys.Clear();
            foreach (var pair in lastHitTimes)
            {
                if (now - pair.Value > keepTime) removeKeys.Add(pair.Key);
            }
            foreach (var key in removeKeys) lastHitTimes.Remove(key);
        }

        // 攻撃の情報から記録のキーを作る。None なら 0(記録しない)
        public static ulong GetHitKey(IALCollisionExtraData attacker)
        {
            switch (attacker.HitHistoryMode)
            {
                case ALHitHistoryMode.PerActivation:
                    return ((ulong)ALHitHistoryMode.PerActivation << 56) | attacker.SerialNumber;
                case ALHitHistoryMode.PerSource:
                case ALHitHistoryMode.PerParticle:
                    {
                        int instanceId = attacker is UnityEngine.Object obj ? obj.GetInstanceID() : attacker.GetHashCode();
                        return ((ulong)ALHitHistoryMode.PerSource << 56) | (uint)instanceId;
                    }
                default:
                    return 0;
            }
        }
    }
}
#nullable restore
