#nullable enable
using afl;

namespace ActorWorkspace.ArcaneLedger
{
    public struct ApplyStatusEffectParams
    {
        // 成功率(0-1)
        public float ApplyChance;

        // 効果時間
        public float DurationTime;

        // Tick間隔時間(ダメージ発生間隔)
        public float TickIntervalTime;

        // public float[] Params = new float[1];
        public UnionPrimitiveData Param1;
        public UnionPrimitiveData Param2;
        public UnionPrimitiveData Param3;

        // public StatusEffectData()
        // {
        //     Params = new float[StatusEffectController.MaxStatusEffectParamCount];
        // }
    }
}
#nullable restore