#nullable enable
using UnityEngine;

namespace ActorWorkspace.ArcaneLedger
{
    // ノックバックの段階と、かけるときのパラメータ(ALKnockbackStatusEffect)
    // 段階は 0 なし・1 以上が強さ。段階の数と、段階ごとの距離・時間はゲームが決める
    public static class ALKnockbackUtility
    {
        // 耐性を引いた段階(0〜maxStage)。耐性 1 につき 1 段階下がる
        public static int ApplyResist(int attackStage, float resist, int maxStage)
        {
            return Mathf.Clamp(attackStage - Mathf.RoundToInt(Mathf.Max(0.0f, resist)), 0, maxStage);
        }

        // ノックバックをかけるパラメータ(Id は CoreStatusEffectId.Knockback。必ずかかる)
        // direction: 押し返す向き(xy の平面。長さは問わない)。distance: 押し返す距離。time: 押し返すのにかかる時間(秒)
        public static ApplyStatusEffectParams CreateApplyParams(Vector2 direction, float distance, float time)
        {
            var normalized = direction.normalized;
            var applyParam = new ApplyStatusEffectParams()
            {
                Id = (int)CoreStatusEffectId.Knockback,
                ApplyChance = 1.0f,
                DurationTime = time,
            };
            applyParam.Param1.Float = normalized.x;
            applyParam.Param2.Float = normalized.y;
            applyParam.Param3.Float = distance;
            return applyParam;
        }
    }
}
#nullable restore
