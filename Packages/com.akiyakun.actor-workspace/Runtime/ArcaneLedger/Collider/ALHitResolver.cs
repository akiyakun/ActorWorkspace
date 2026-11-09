#nullable enable
using System.Collections.Generic;
using afl;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // 当たりの候補を溜めて、まとめて確定する(ALHurtBoxBehaviour が使う)
    // 1. 陣営: 攻撃した側の陣営が HostileMask に入っていなければ当たらない
    // 2. HitHistoryMode が None の攻撃は、候補ごとにそのまま当たる(今までと同じ)
    // 3. それ以外は、同じ攻撃(HitKey)の候補を 1 つにまとめる。HurtBox の Priority がいちばん大きいもの(同じなら先に来たもの)を使う
    // 4. ヒット履歴(ALHitHistory)で、当たり直してよいかを確かめてから当たる
    public sealed class ALHitResolver
    {
        struct Candidate
        {
            public CollisionContactInfo Raw;
            public IALCollisionExtraData Attacker;
            public IALCollisionExtraData? HurtBox;
        }

        readonly List<Candidate> candidates = new();
        readonly List<ALHitContact> resolved = new();

        public ALHitHistory History { get; } = new();
        public int PendingCount => candidates.Count;

        public void Add(in CollisionContactInfo raw, IALCollisionExtraData attacker, IALCollisionExtraData? hurtBox)
        {
            candidates.Add(new Candidate() { Raw = raw, Attacker = attacker, HurtBox = hurtBox });
        }

        // 溜めた候補を確定する。戻り値の一覧は次の Resolve() まで使える
        // now: ヒット履歴の時刻(ワールドを止めている間は進まない時間にする)
        public IReadOnlyList<ALHitContact> Resolve(float now, uint hostileMask)
        {
            resolved.Clear();

            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (ALFaction.CanHit(candidate.Attacker.Faction, hostileMask) == false) continue;

                ulong key = ALHitHistory.GetHitKey(candidate.Attacker);
                if (candidate.Attacker.HitHistoryMode == ALHitHistoryMode.None)
                {
                    resolved.Add(ToContact(candidate, key));
                    continue;
                }

                // 同じ攻撃で、先に出た候補があればそちらでまとめたので飛ばす
                if (IsAlreadyGrouped(i, key)) continue;

                // 同じ攻撃の候補の中で、優先度がいちばん大きい HurtBox を使う
                var best = candidate;
                for (int j = i + 1; j < candidates.Count; j++)
                {
                    var other = candidates[j];
                    if (other.Attacker.HitHistoryMode == ALHitHistoryMode.None) continue;
                    if (ALHitHistory.GetHitKey(other.Attacker) != key) continue;
                    if (GetPriority(other) > GetPriority(best)) best = other;
                }

                if (History.CanHit(key, best.Attacker.RepeatHitInterval, now) == false) continue;
                History.Mark(key, now);
                resolved.Add(ToContact(best, key));
            }

            candidates.Clear();
            return resolved;
        }

        public void Clear()
        {
            candidates.Clear();
            resolved.Clear();
            History.Clear();
        }

        bool IsAlreadyGrouped(int index, ulong key)
        {
            for (int j = 0; j < index; j++)
            {
                var prev = candidates[j];
                if (prev.Attacker.HitHistoryMode == ALHitHistoryMode.None) continue;
                if (ALHitHistory.GetHitKey(prev.Attacker) == key) return true;
            }
            return false;
        }

        static int GetPriority(in Candidate candidate) => candidate.HurtBox?.Priority ?? 0;

        static ALHitContact ToContact(in Candidate candidate, ulong key)
        {
            return new ALHitContact() { Raw = candidate.Raw, Attacker = candidate.Attacker, HurtBox = candidate.HurtBox, HitKey = key };
        }
    }
}
#nullable restore
