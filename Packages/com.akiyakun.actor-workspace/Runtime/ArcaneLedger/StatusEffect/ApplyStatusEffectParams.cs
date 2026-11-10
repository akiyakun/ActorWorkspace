#nullable enable
using afl;

namespace ActorWorkspace.ArcaneLedger
{
    public struct ApplyStatusEffectParams
    {
        // ステータス効果Id
        public int Id;

        // 成功率(0-1)
        public float ApplyChance;

        // 効果時間
        public float DurationTime;

        // Tick間隔時間(ダメージ発生間隔)
        public float TickIntervalTime;

        public UnionPrimitiveData Param1;
        public UnionPrimitiveData Param2;
        public UnionPrimitiveData Param3;

        // かけたアクター(分からなければ null)。続くダメージ(ALDamageOverTimeStatusEffectBase)の ALDamageInfo.Attacker になる
        // MEMO: 倒したときの ALActorEvents.OnKilled・OnDealtDamage が、かけたアクターに届く(撃破数・経験値を誰のものにするか)
        public IAWActor? Applier;
    }
}
#nullable restore