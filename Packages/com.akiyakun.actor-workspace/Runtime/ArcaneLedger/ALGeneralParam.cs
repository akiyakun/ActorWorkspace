#nullable enable
using System.Runtime.CompilerServices;
using UnityEngine;
using afl;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;

namespace ActorWorkspace.ArcaneLedger
{
    public class ALGeneralParam
    {
        public const int MaxParamCount = AWCoreAnimationEvents.MaxGenParamCount;

        /*
        基礎値の計算後の結果である一時的な値をAとする

        値[Value]:基礎値を値
        係数[Factor]:AにNを掛ける(0の場合0になる)
        増減値[Modifier]:AにNを加える
        補正値[Correction]:Aに対し修正を行う場合に使用する値
        */
        public class Data
        {
            public float Value;
            public float Factor;
            public float Delta;
            // public float Modifier;

            public Data()
            {
                Restore();
            }

            public void Restore()
            {
                Value = 0.0f;
                Factor = 1.0f;
                Delta = 0.0f;
            }
        }

        Data[] dataArray = new Data[MaxParamCount];

        // 今のアニメーションで、イベント(SetGenValue[X]・SetGenFactor[X]・SetGenDelta[X])が入れた汎用パラメータのビット(1 << id)
        // MEMO: MotionValueController が入れ、次のアニメーションに入ると 0 に戻す。値が既定のままか、イベントで 0 を入れたかを区別するのに使う
        public uint EventSetMask { get; set; }
        public bool IsSetByEvent(int id) => (EventSetMask & (1u << id)) != 0;

        // 汎用パラメータの計算((Value + Delta) × Factor)。写し(ALGeneralParamSnapshot.Calc)も同じ式
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Calc(float value, float factor, float delta) => (value + delta) * factor;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Calc(int id)
        {
            var data = this[id];
            return ALGeneralParam.Calc(data.Value, data.Factor, data.Delta);
        }
        // public Data[] GeneralParams => generalParams;

        public ALGeneralParam()
        {
            for (int i = 0; i < dataArray.Length; i++)
            {
                dataArray[i] = new Data();
            }
        }

        public void Restore()
        {
            for (int i = 0; i < dataArray.Length; i++)
            {
                dataArray[i].Restore();
            }
            EventSetMask = 0;
        }

        public Data this[int i]
        {
            get
            {
                Debug.Assert(i >= 0 && i < MaxParamCount);
                return dataArray[i];
            }
        }

    }
}
#nullable restore