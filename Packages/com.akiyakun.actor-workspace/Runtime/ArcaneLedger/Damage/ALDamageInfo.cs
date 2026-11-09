#nullable enable
using UnityEngine;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;

namespace ActorWorkspace.ArcaneLedger
{
    // ダメージの種類
    // MEMO: ゲーム側で数値として保存することがあるので、末尾に追加すること
    public enum ALDamageKind
    {
        Hit = 0,            // 攻撃(無敵の間は受けない。受けると無敵になることがある)
        Continuous = 1,     // 続くダメージ(毒・地形など。無敵の間も受け、受けても無敵にならない)
        Direct = 2,         // 演出・デバッグなど(無敵の間も受け、受けても無敵にならない)
    }

    // ダメージの情報(ALActorEvents.RequestDamage、DefaultALProcessor.ApplyDamage() に渡す)
    public struct ALDamageInfo
    {
        public ALDamageKind Kind;
        // 攻撃したアクター(分からなければ null)
        public IAWActor? Attacker;
        // 当たった HitBox・弾の情報(当たり判定によらないダメージは null)
        public IALCollisionExtraData? Source;
        // ゲームが決める攻撃の ID(マスタの Id・スキルの Id など。0 はなし)
        public int AttackId;
        // ダメージの量。DefaultALProcessor.CalcDamage() の入力で、計算のあとは最終的なダメージ
        public float Value;
        // ノックバックなどのリアクションの段階(0 はなし)
        public int DamageReaction;
        // 当たった位置
        public Vector3 Position;
        // ゲームが決めるフラグ(属性など)
        public uint Flags;
        public object? UserData;

        // 攻撃のダメージ
        public static ALDamageInfo Hit(float value, IAWActor? attacker = null)
        {
            return new ALDamageInfo() { Kind = ALDamageKind.Hit, Value = value, Attacker = attacker };
        }

        // 続くダメージ(毒・地形など)
        public static ALDamageInfo Continuous(float value, IAWActor? attacker = null)
        {
            return new ALDamageInfo() { Kind = ALDamageKind.Continuous, Value = value, Attacker = attacker };
        }
    }
}
#nullable restore
