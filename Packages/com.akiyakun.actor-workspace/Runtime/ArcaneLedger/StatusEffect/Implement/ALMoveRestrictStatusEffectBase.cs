#nullable enable
using UnityEngine;

namespace ActorWorkspace.ArcaneLedger
{
    /*
        移動を制限するステータス効果の基底(こおり・どんそく・まひ・こんらん・きょうふ など)
        Param1: 減速の割合(0.0:減速しない、0.5:半分の速さ、1.0:止まる)

        かかっている間、移動速度のステータス(IALProcessor.Config.MoveSpeedStatId)に MultiplyMin の補正を付ける
        (複数の効果がかかっているときは、いちばん遅くなるものを使う)
        MEMO: マスタで空欄(0)のときに止まらないように、倍率ではなく減速の割合を渡す
    */
    public abstract class ALMoveRestrictStatusEffectBase : StatusEffect
    {
        public override bool Apply(ApplyStatusEffectParams applyParam)
        {
            bool result = base.Apply(applyParam);

            // かけ直したときは、新しい倍率にする
            // MEMO: この効果が AddModifierWhileEnabled() で付けた補正は、ここですべて付け直す
            RemoveModifiersWhileEnabled();
            float factor = GetMoveSpeedFactor(applyParam);
            if (factor < 1.0f) AddModifierWhileEnabled(controller.MoveSpeedStatId, ALModifierOp.MultiplyMin, factor);

            return result;
        }

        // 移動速度の倍率(0.0〜1.0)
        protected virtual float GetMoveSpeedFactor(ApplyStatusEffectParams applyParam) => 1.0f - Mathf.Clamp01(applyParam.Param1.Float);

        // 移動の向きを上書きする。優先度の高い(Id が小さい)効果が先に上書きしていたら、そちらを優先する
        // MEMO: 上書きは毎フレーム戻るので、OnUpdate() で毎回呼ぶ
        protected void OverrideMoveDirectionIfEmpty(Vector3 direction)
        {
            if (Ledger.OverrideMoveDirection != Vector3.zero) return;
            Ledger.OverrideMoveDirection = direction;
        }
    }
}
#nullable restore
