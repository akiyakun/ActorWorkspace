#nullable enable
namespace ActorWorkspace.ArcaneLedger
{
    // 汎用パラメータ(ALGeneralParam)の、ある時点の写し
    // 当たった瞬間の、攻撃した側・受けた側の値(モーション値など)を、当たりを確定するまで取っておくのに使う(ALHitContact・ALDamageInfo)
    // MEMO: 写しは ALHitResolver の作業用の配列を指す。使えるのは、その写しを返した Resolve() の次の Resolve() まで
    public readonly struct ALGeneralParamSnapshot
    {
        // 1 つの写しが使う float の数(Value・Factor・Delta × 汎用パラメータの数)
        public const int Stride = ALGeneralParam.MaxParamCount * 3;

        readonly float[]? buffer;
        readonly int offset;

        // 写したときの ALGeneralParam.EventSetMask(今のアニメーションで、イベントが入れた汎用パラメータのビット)
        public uint EventSetMask { get; }
        public bool IsSetByEvent(int id) => (EventSetMask & (1u << id)) != 0;

        public ALGeneralParamSnapshot(float[] buffer, int offset, uint eventSetMask = 0)
        {
            this.buffer = buffer;
            this.offset = offset;
            EventSetMask = eventSetMask;
        }

        // 写しがあるか(ALProcessor を持たない当たり判定などは無い)
        public bool IsValid => buffer != null;

        public float Value(int id) => Get(id, 0);
        public float Factor(int id) => Get(id, 1);
        public float Delta(int id) => Get(id, 2);
        // DefaultALProcessor.Calc() と同じ計算
        public float Calc(int id) => (Value(id) + Delta(id)) * Factor(id);

        float Get(int id, int field)
        {
            if (buffer == null) throw new System.InvalidOperationException("ALGeneralParamSnapshot: 写しがありません(IsValid を確かめてください)");
            if (id < 0 || id >= ALGeneralParam.MaxParamCount) throw new System.ArgumentOutOfRangeException(nameof(id));
            return buffer[offset + id * 3 + field];
        }

        // source を buffer の offset から Stride 個に写す
        public static void Write(ALGeneralParam source, float[] buffer, int offset)
        {
            for (int id = 0; id < ALGeneralParam.MaxParamCount; id++)
            {
                var data = source[id];
                int index = offset + id * 3;
                buffer[index + 0] = data.Value;
                buffer[index + 1] = data.Factor;
                buffer[index + 2] = data.Delta;
            }
        }
    }
}
#nullable restore
