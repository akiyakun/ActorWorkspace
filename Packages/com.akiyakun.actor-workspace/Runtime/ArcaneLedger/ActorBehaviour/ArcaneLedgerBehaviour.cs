#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;
using ActorWorkspace.ArcaneLedger;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    public class ArcaneLedgerBehaviour<TALProcessor> : ArcaneLedgerBehaviour
        where TALProcessor : class, IALProcessor, new()
    {
        public override IALProcessor Processor { get; } = new TALProcessor();
    }

    // アーケイン・レジャー
    // アクターの汎用パラメータ・ステータス・HP・状態異常を持ち、計算・判定は Processor(IALProcessor)に任せる
    // FIXME: 後で整理する。雑多に処理入れてる
    public abstract class ArcaneLedgerBehaviour : AWActorBehaviour<IAWActor>
    {
        public abstract IALProcessor Processor { get; }

        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Update | UpdateFlags.FixedUpdate;

        public ALGeneralParam GeneralParam { get; } = new ALGeneralParam();

        // ステータス(大きさは Processor.Config.StatCapacity)・HP(最大 HP は Processor.Config.MaxLifeStatId のステータス)
        // MEMO: OnAwake() で作る
        public ALStatTable Stats { get; private set; } = null!;
        public ALHealth Health { get; private set; } = null!;

        // ステータス効果が決める、本来の移動の向きの代わりに使う向き(Vector3.zero なら変えない)
        // 毎フレームの Update の始めに Vector3.zero に戻り、ステータス効果の更新で設定し直す(Id が小さい効果ほど優先)
        // MEMO: 移動の処理は ArcaneLedgerBehaviour より後で更新して、これを読む
        public Vector3 OverrideMoveDirection { get; set; }

        // 陣営(ALFaction。0〜31)と、当たる相手の陣営のビット(既定はすべての陣営に当たる)
        // MEMO: 攻撃の当たり判定(HitBox・弾)の Faction がこのアクターの HostileMask に入っていれば、このアクターに当たる
        public int Faction { get; set; }
        public uint HostileMask { get; set; } = ALFaction.All;

        MotionValueController motionValueController = null!;
        StatusEffectController statusEffectController = null!;


        public override void OnRestore()
        {
            motionValueController.Restore();
            statusEffectController.Restore();
            // MEMO: 補正を外してから HP を最大にする(最大 HP の補正が残らないように)
            // MEMO: Processor.Restore() で最大 HP の補正を付け直すとき(ソウルスキルなど)は、HP はその前の最大 HP のままなので、Processor 側で Health.Reset() し直す
            Stats.Restore();
            Health.Reset();
            OverrideMoveDirection = Vector3.zero;
            Processor.Restore();
            GeneralParam.Restore();
        }

        public override void OnAwake()
        {
            var config = Processor.Config;
            Stats = new ALStatTable(config.StatCapacity);
            Health = new ALHealth(Stats, config.MaxLifeStatId);

            motionValueController = new MotionValueController(this);
            statusEffectController = new StatusEffectController(this);

            Processor.Setup(this, statusEffectController);
        }

        // 購読を外す(Processor の EventBag・MotionValueController)
        public override void OnDestroy()
        {
            motionValueController?.Dispose();
            (Processor as System.IDisposable)?.Dispose();
        }

        public override void OnUpdate(float deltaTime)
        {
            OverrideMoveDirection = Vector3.zero;

            statusEffectController.Prepare();
            Processor.Prepare();

            statusEffectController.Update(deltaTime);
            Processor.Update(deltaTime);

            Health.Update(deltaTime);
        }

        public override void OnFixedUpdate(float deltaTime)
        {
            statusEffectController.FixedUpdate(deltaTime);
            Processor.FixedUpdate(deltaTime);
        }

        // Call from ALHurtBoxBehaviour(陣営・ヒット履歴・部位を確かめたあと)
        // 当たったら、リアクションの段階を AWCoreActorEvents.DamageReaction で知らせる(0 もそのまま知らせる)
        public void ContactWithHurtBox(in ALHitContact contact)
        {
            var hitResult = Processor.Hit(contact);
            if (hitResult.IsHit == false) return;

            EventBus.Publish(AWCoreActorEvents.DamageReaction, hitResult.DamageReaction);
        }
    }
}
#nullable restore