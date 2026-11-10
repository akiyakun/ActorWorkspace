#nullable enable
using UnityEngine;
using afl;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger
{
    // コライダーがアタッチされたオブジェクトから必要な情報を保持・取得するためのインターフェース
    // HitBox(攻撃する側)と HurtBox(受ける側)の両方に付く。項目によって、どちらで使うかが違う
    public interface IALCollisionExtraData
    {
        public IAWActor? Actor { get; }
        public IALProcessor? ALProcessor { get; }

        // HitBox が出るたび・エフェクトを再生するたびに振り直す番号(ColliderSerialNumberGenerator)
        public uint SerialNumber { get; }
        public uint Flags { get; }

        public int IntValue { get; }
        public float FloatValue { get; }
        public string StringValue { get; }
        public object? UserData { get; }

        #region HitBox(攻撃する側)
        // 攻撃した側の陣営(ALFaction)。受ける側の ArcaneLedgerBehaviour.HostileMask で当たるかを決める
        public int Faction { get; }
        // ゲームが決める攻撃の ID(0 はなし)
        public int AttackId { get; }
        // 同じ相手に何度も当たるのを防ぐ方法と、当たり直すまでの秒数(ALHitHistory)
        public ALHitHistoryMode HitHistoryMode { get; }
        public float RepeatHitInterval { get; }
        #endregion

        #region HurtBox(受ける側)
        // 部位(0 はなし)・部位のダメージ倍率・同じフレームに複数の部位に当たったときの優先度(大きいほうを使う)
        public int Region { get; }
        public float DamageMultiplier { get; }
        public int Priority { get; }
        #endregion

#if UNITY_EDITOR
        public bool DebugBreakPause { get; set; }
#endif
    }
}
#nullable restore
