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