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
        // 攻撃したアクターの ALProcessor(当たったときの値。分からなければ null)
        public IALProcessor? AttackerProcessor;
        // 当たった HitBox・弾の情報(当たり判定によらないダメージは null)
        // MEMO: 確定したときの中身は、当たったときと違うことがある。攻撃したアクター・ALProcessor・攻撃の ID は上の値を使う
        public IALCollisionExtraData? Source;
        // ゲームが決める攻撃の ID(マスタの Id・スキルの Id など。0 はなし)
        public int AttackId;
        // ダメージの量。DefaultALProcessor.CalcDamage() の入力で、計算のあとは最終的なダメージ
        public float Value;
        // ノックバックなどのリアクションの段階(0 はなし)
        public int DamageReaction;
        // 当たった位置
        public Vector3 Position;
        // 当たった部位(0 はなし)と部位のダメージ倍率(ALHitContact から。当たり判定によらないダメージは 1)
        // MEMO: 倍率は CalcDamage() で使うかをゲームが決める(既定の CalcDamage は使わない)
        public int Region;
        public float RegionMultiplier;
        // 当たった瞬間の、攻撃した側・受けた側の汎用パラメータの写し(FromHit() のとき。当たり判定によらないダメージは IsValid が false)
        // MEMO: 使えるのは、当たりを確定した ALHurtBoxBehaviour の Update の中だけ(ALGeneralParamSnapshot)
        public ALGeneralParamSnapshot AttackerParams;
        public ALGeneralParamSnapshot ReceiverParams;
        // ゲームが決めるフラグ(属性など)
        public uint Flags;
        public object? UserData;

        // 攻撃のダメージ
        public static ALDamageInfo Hit(float value, IAWActor? attacker = null)
        {
            return new ALDamageInfo() { Kind = ALDamageKind.Hit, Value = value, Attacker = attacker, RegionMultiplier = 1.0f };
        }

        // 続くダメージ(毒・地形など)
        public static ALDamageInfo Continuous(float value, IAWActor? attacker = null)
        {
            return new ALDamageInfo() { Kind = ALDamageKind.Continuous, Value = value, Attacker = attacker, RegionMultiplier = 1.0f };
        }

        // 当たり判定の当たりからの攻撃のダメージ(攻撃したアクター・当たり判定・攻撃の ID・部位・位置を入れる)
        public static ALDamageInfo FromHit(in ActorBehaviour.ALHitContact contact, float value)
        {
            return new ALDamageInfo()
            {
                Kind = ALDamageKind.Hit,
                Value = value,
                Attacker = contact.AttackerActor,
                AttackerProcessor = contact.AttackerProcessor,
                Source = contact.Attacker,
                AttackId = contact.AttackId,
                Region = contact.Region,
                RegionMultiplier = contact.RegionMultiplier,
                Position = contact.Raw.Position,
                AttackerParams = contact.AttackerParams,
                ReceiverParams = contact.ReceiverParams,
            };
        }
    }
}
#nullable restore
