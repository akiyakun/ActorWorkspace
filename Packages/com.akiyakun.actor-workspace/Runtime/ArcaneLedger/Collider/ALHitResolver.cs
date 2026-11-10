#nullable enable
using System.Collections.Generic;
using afl;

namespace ActorWorkspace.ArcaneLedger
{
    // 当たりの候補を溜めて、まとめて確定する(ALHurtBoxBehaviour が使う)
    // 0. IgnoreEvents の種類の通知(既定は「離れた」(Exit))は候補にしない
    // 1. 陣営: 攻撃した側の陣営が HostileMask に入っていなければ当たらない
    // 2. HitHistoryMode が None の攻撃は、候補ごとにそのまま当たる(今までと同じ)
    // 3. それ以外は、同じ攻撃(HitKey)の候補を 1 つにまとめる。HurtBox の Priority がいちばん大きいもの(同じなら先に来たもの)を使う
    // 4. ヒット履歴(ALHitHistory)で、当たり直してよいかを確かめてから当たる
    // Add() のときに、攻撃した側と受けた側(ReceiverParams)の汎用パラメータを写しておく(ALHitContact.AttackerParams・ReceiverParams)
    // MEMO: 確定するのは Update なので、それまでにモーションが変わってモーション値などが戻っても、当たった瞬間の値で計算できる
    // MEMO: 攻撃した側の陣営・ヒット履歴の設定・攻撃の ID・アクター・ALProcessor も Add() のときに写す
    //       (確定までに、攻撃が次のアニメーションで元に戻ったり、弾がプールに戻って中身が消えたり、アクターが消えたりするため)
    public sealed class ALHitResolver
    {
        struct Candidate
        {
            public CollisionContactInfo Raw;
            public IALCollisionExtraData Source;
            public IALCollisionExtraData? HurtBox;
            // Add() のときの攻撃した側の値
            public int Faction;
            public ALHitHistoryMode HitHistoryMode;
            public float RepeatHitInterval;
            public ulong HitKey;
            public int AttackId;
            public IAWActor? Attacker;
            public IALProcessor? AttackerProcessor;
            // 汎用パラメータの写しの位置(無ければ -1)と、写したときの EventSetMask
            public int AttackerParamsOffset;
            public int ReceiverParamsOffset;
            public uint AttackerEventSetMask;
            public uint ReceiverEventSetMask;
        }

        readonly List<Candidate> candidates = new();
        readonly List<ALHitContact> resolved = new();

        public ALHitHistory History { get; } = new();
        public int PendingCount => candidates.Count;

        // 受けた側の汎用パラメータ(写しを取る。null なら取らない)
        public ALGeneralParam? ReceiverParams { get; set; }

        // 受けた側の、当たる相手の陣営のビット(ArcaneLedgerBehaviour.HostileMask)
        // MEMO: Add() のときに、陣営が当たらない候補を捨てる(汎用パラメータを写さないため。Resolve() でも確かめる)
        public uint HostileMask { get; set; } = ALFaction.All;

        // 汎用パラメータの写しを入れる配列。Add() は writeBuffer に写し、Resolve() で入れ替える
        // (Resolve() が返した当たりの写しは、次の Resolve() まで書き換えない)
        float[] writeBuffer = new float[ALGeneralParamSnapshot.Stride * 4];
        float[] readBuffer = new float[ALGeneralParamSnapshot.Stride * 4];
        int writeCount;

        // 当たりにしない通知の種類(既定は「離れた」(Exit)。入ったとき・重なっている間・パーティクルの当たりは当たりにする)
        // MEMO: Exit も当たりにすると、1 回の攻撃が入ったときと離れたときの 2 回当たる
        public CollisionEventType IgnoreEvents { get; set; } = DefaultIgnoreEvents;
        public const CollisionEventType DefaultIgnoreEvents =
            CollisionEventType.Collision3DExit | CollisionEventType.Trigger3DExit |
            CollisionEventType.Collision2DExit | CollisionEventType.Trigger2DExit;

        // source: 攻撃の当たり判定(HitBox・弾)、hurtBox: 受けた側の HurtBox(分からなければ null)
        // 戻り値: 候補にしたか(IgnoreEvents の種類・当たらないと分かっているときは false)
        public bool Add(in CollisionContactInfo raw, IALCollisionExtraData source, IALCollisionExtraData? hurtBox)
        {
            if ((raw.EventType & IgnoreEvents) != 0) return false;

            // 当たらないと分かっている候補は、汎用パラメータを写す前に捨てる
            // - 陣営が当たらない
            // - 当たり直さない攻撃(RepeatHitInterval が 0 以下)で、もう当たっている
            // MEMO: 間隔のある攻撃は、確定する時刻で決まるので Resolve() で確かめる
            int faction = source.Faction;
            if (ALFaction.CanHit(faction, HostileMask) == false) return false;
            var hitHistoryMode = source.HitHistoryMode;
            float repeatHitInterval = source.RepeatHitInterval;
            ulong hitKey = ALHitHistory.GetHitKey(source);
            if (hitHistoryMode != ALHitHistoryMode.None && repeatHitInterval <= 0.0f && History.CanHit(hitKey, repeatHitInterval, 0.0f) == false) return false;

            var processor = source.ALProcessor;
            candidates.Add(new Candidate()
            {
                Raw = raw,
                Source = source,
                HurtBox = hurtBox,
                Faction = faction,
                HitHistoryMode = hitHistoryMode,
                RepeatHitInterval = repeatHitInterval,
                HitKey = hitKey,
                AttackId = source.AttackId,
                Attacker = source.Actor,
                AttackerProcessor = processor,
                AttackerParamsOffset = WriteSnapshot(processor?.GeneralParam),
                ReceiverParamsOffset = WriteSnapshot(ReceiverParams),
                AttackerEventSetMask = processor?.GeneralParam?.EventSetMask ?? 0,
                ReceiverEventSetMask = ReceiverParams?.EventSetMask ?? 0,
            });
            return true;
        }

        int WriteSnapshot(ALGeneralParam? source)
        {
            if (source == null) return -1;
            int offset = writeCount;
            if (offset + ALGeneralParamSnapshot.Stride > writeBuffer.Length)
            {
                System.Array.Resize(ref writeBuffer, System.Math.Max(writeBuffer.Length * 2, offset + ALGeneralParamSnapshot.Stride));
            }
            ALGeneralParamSnapshot.Write(source, writeBuffer, offset);
            writeCount += ALGeneralParamSnapshot.Stride;
            return offset;
        }

        // 溜めた候補を確定する。戻り値の一覧は次の Resolve() まで使える
        // now: ヒット履歴の時刻(ワールドを止めている間は進まない時間にする)
        public IReadOnlyList<ALHitContact> Resolve(float now, uint hostileMask)
        {
            resolved.Clear();

            try
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    var candidate = candidates[i];
                    if (ALFaction.CanHit(candidate.Faction, hostileMask) == false) continue;

                    ulong key = candidate.HitKey;
                    if (candidate.HitHistoryMode == ALHitHistoryMode.None)
                    {
                        resolved.Add(ToContact(candidate));
                        continue;
                    }

                    // 同じ攻撃で、先に出た候補があればそちらでまとめたので飛ばす
                    if (IsAlreadyGrouped(i, key)) continue;

                    // 同じ攻撃の候補の中で、優先度がいちばん大きい HurtBox を使う
                    var best = candidate;
                    for (int j = i + 1; j < candidates.Count; j++)
                    {
                        var other = candidates[j];
                        if (other.HitHistoryMode == ALHitHistoryMode.None) continue;
                        if (other.HitKey != key) continue;
                        if (GetPriority(other) > GetPriority(best)) best = other;
                    }

                    if (History.CanHit(key, best.RepeatHitInterval, now) == false) continue;
                    History.Mark(key, now);
                    resolved.Add(ToContact(best));
                }
            }
            finally
            {
                // 途中で例外が出ても、溜めた候補は捨てる(次のフレームに持ち越さない)
                candidates.Clear();
                // 返した当たりの写しは readBuffer に残し、次の Add() からは別の配列に写す
                (writeBuffer, readBuffer) = (readBuffer, writeBuffer);
                writeCount = 0;
            }
            return resolved;
        }

        public void Clear()
        {
            candidates.Clear();
            resolved.Clear();
            History.Clear();
            writeCount = 0;
        }

        bool IsAlreadyGrouped(int index, ulong key)
        {
            for (int j = 0; j < index; j++)
            {
                var prev = candidates[j];
                if (prev.HitHistoryMode == ALHitHistoryMode.None) continue;
                if (prev.HitKey == key) return true;
            }
            return false;
        }

        static int GetPriority(in Candidate candidate) => candidate.HurtBox?.Priority ?? 0;

        ALHitContact ToContact(in Candidate candidate)
        {
            return new ALHitContact()
            {
                Raw = candidate.Raw,
                Source = candidate.Source,
                HurtBox = candidate.HurtBox,
                HitKey = candidate.HitKey,
                AttackId = candidate.AttackId,
                Attacker = candidate.Attacker,
                AttackerProcessor = candidate.AttackerProcessor,
                AttackerParams = ToSnapshot(candidate.AttackerParamsOffset, candidate.AttackerEventSetMask),
                ReceiverParams = ToSnapshot(candidate.ReceiverParamsOffset, candidate.ReceiverEventSetMask),
            };
        }

        ALGeneralParamSnapshot ToSnapshot(int offset, uint eventSetMask) => offset >= 0 ? new ALGeneralParamSnapshot(writeBuffer, offset, eventSetMask) : default;
    }
}
#nullable restore
