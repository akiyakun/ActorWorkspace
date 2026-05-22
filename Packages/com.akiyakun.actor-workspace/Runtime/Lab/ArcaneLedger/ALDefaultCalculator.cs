#nullable enable
using System.Runtime.CompilerServices;
using UnityEngine;
using afl;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;

namespace ActorWorkspace.ArcaneLedger
{
    /*
        基礎値の計算後の結果である一時的な値をAとする

        値[Value]:基礎値を値
        係数[Factor]:AにNを掛ける(0の場合0になる)
        増減値[Modifier]:AにNを加える
        補正値[Correction]:Aに対し修正を行う場合に使用する値
    */
    public class ALGeneralParam
    {
        public const int MaxParamCount = AWCoreAnimationEvents.MaxGenParamCount;

        public float Value;
        public float Factor;
        public float Modifier;

        public ALGeneralParam()
        {
            Restore();
        }

        public void Restore()
        {
            Value = 0.0f;
            Factor = 1.0f;
            Modifier = 0.0f;
        }
    }

    /* General Params List
        D: ダメージリアクション調整値
    */
    public class ALDefaultCalculator : IALCalculator
    {
        public enum ParamId
        {
            A = 0,
            B,
            C,

            // D
            DamageReaction,
            // DamageReactionModifier = 0,
            // E
            DamageReactionResist,

            F,
            G,
            H,
            I,
            J,
            K,
            L,
            M,
            N,
            O,
            P,
            Q,
            R,
            S,
            T,
            U,
            V,
            W,
            X,
            Y,
            Z,
        }

        ALGeneralParam[] generalParams = new ALGeneralParam[ALGeneralParam.MaxParamCount];
        public ALGeneralParam[] GeneralParams => generalParams;

        IAWActor me;

        public ALDefaultCalculator(IAWActor actor)
        {
            me = actor;

            for (int i = 0; i < generalParams.Length; i++)
            {
                generalParams[i] = new ALGeneralParam();
            }
        }

        public void Restore()
        {
            for (int i = 0; i < generalParams.Length; i++)
            {
                generalParams[i].Restore();
            }
        }

        public ALGeneralParam GetGeneralParam(int paramId)
        {
            return generalParams[paramId];
        }

        public ALGeneralParam GetGeneralParam(ParamId type)
        {
            return generalParams[(int)type];
        }

        public HitResult Hit(CollisionContactInfo contactInfo)
        {
            // me: ステータスの取得
            // me: モーション値の取得
            // other: ステータスの取得
            // other: ヒットボックスの情報から攻撃の強さや属性を取得

            if (contactInfo.Other.TryGetComponent<ALColliderExtraInfo>(out var otherInfo) == false)
            {
                return HitResult.NotHit;
            }

            var otherCalculator = otherInfo.Actor.ALCalculator;
            if (otherCalculator == null) return HitResult.NotHit;

            float meValue = Calc((int)ParamId.DamageReactionResist);
            float otherValue = otherCalculator.Calc((int)ParamId.DamageReaction);
            var result = Mathf.Max(0.0f, otherValue - meValue);

            // 録画用
            // float v = otherCalculator.GetGeneralParam((int)ParamId.DamageReaction).Value;
            // v += 1.0f;
            // if (v >= 3.0f) v = 0.0f;
            // otherCalculator.GetGeneralParam((int)ParamId.DamageReaction).Value = v;

            return new HitResult() { IsHit = true, Value = result };
        }


        // デフォルトの汎用パラメータ計算
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Calc(int paramId)
        {
            var param = GeneralParams[paramId];
            return (param.Value + param.Modifier) * param.Factor;
        }

        // protected virtual float CalcDamageReaction()
        // {
        //     var param = GetGeneralParam(GeneralParamType.DamageReaction);
        //     return param.Value + param.Modifier;
        // }

        // protected virtual float CalcDamageReactionResist()
        // {
        //     var param = GetGeneralParam(GeneralParamType.DamageReactionResist);
        //     return param.Value + param.Modifier;
        // }

    }
}
#nullable restore