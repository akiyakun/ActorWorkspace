#nullable enable
using UnityEngine;

namespace ActorWorkspace.ArcaneLedger
{
    /*
        ノックバック(押し返す)。パラメータは ALKnockbackUtility.CreateApplyParams() で作る
        Param1・Param2: 押し返す向きの x・y(長さ 1)
        Param3: 押し返す距離
        DurationTime: 押し返すのにかかる時間(秒。0 なら次の更新で一度に動かす)

        押し返している間は、自分では動かない(移動速度のステータスに MultiplyMin 0 の補正を付ける)
        ほかのステータス効果は、押し返し終わってからかかる(RequestMask で待たせる。RequestUnmask で外したものは待たせない)
        動かし方は Move() で変えられる(既定は transform.position を動かす)
    */
    public class ALKnockbackStatusEffect : StatusEffectKnockbackBase
    {
        Vector3 direction;
        float distance;
        float duration;
        // 押し返し始めてからの時間と、そこまでに動かした割合(0〜1)
        float elapsed;
        float movedRatio;

        public override bool Apply(ApplyStatusEffectParams applyParam)
        {
            // かかっている間にまたかけたときは、今の位置から押し返し直す
            direction = new Vector3(applyParam.Param1.Float, applyParam.Param2.Float, 0.0f);
            distance = Mathf.Max(0.0f, applyParam.Param3.Float);
            duration = Mathf.Max(0.0f, applyParam.DurationTime);
            elapsed = 0.0f;
            movedRatio = 0.0f;

            base.Apply(applyParam);
            // 終わりは OnUpdate で決める(効果時間で終わると、最後のフレームの残りの距離を動かせないことがある)
            RemainingTime = -1.0f;

            // 押し返している間は自分では動かない
            RemoveModifiersWhileEnabled();
            AddModifierWhileEnabled(controller.MoveSpeedStatId, ALModifierOp.MultiplyMin, 0.0f);

            // 同じフレームにかかるほかのステータス効果も、押し返し終わるまで待たせる
            // MEMO: マスクは毎フレーム StatusEffectController.Prepare() で戻り、Prepare() でかけ直す
            controller.RequestMask |= ~(1u << Id);
            return true;
        }

        public override void Prepare()
        {
            base.Prepare();

            // 押し返している間は、ほかのステータス効果を待たせる
            if (Enable)
            {
                controller.RequestMask |= ~(1u << Id);
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            elapsed += deltaTime;

            // 少しずつ遅くなるように動かす(ease out)。時間が 0 なら一度に動かす
            float t = duration > 0.0f ? Mathf.Clamp01(elapsed / duration) : 1.0f;
            float ratio = 1.0f - (1.0f - t) * (1.0f - t);
            Move(direction * (distance * (ratio - movedRatio)));
            movedRatio = ratio;

            if (t >= 1.0f) Enable = false;
        }

        // アクターを動かす
        protected virtual void Move(Vector3 delta)
        {
            Actor.GameObject.transform.position += delta;
        }
    }
}
#nullable restore
