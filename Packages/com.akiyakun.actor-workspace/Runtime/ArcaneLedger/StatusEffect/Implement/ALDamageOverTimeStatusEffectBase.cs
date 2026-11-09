#nullable enable

namespace ActorWorkspace.ArcaneLedger
{
    /*
        一定間隔でダメージを受けるステータス効果の基底(どく・やけど・地形 など)
        Param1: 減速の割合(ALMoveRestrictStatusEffectBase。減速しないなら 0)
        Param2: 1 回のダメージ
        TickIntervalTime: ダメージの間隔(秒)。0 ならダメージなし

        Tick ごとに、自分に ALActorEvents.RequestDamage(ALDamageKind.Continuous)を発行する
        かけ直したときは次のダメージまでの時間を引き継ぎ、最初のダメージまでの時間は GetFirstTickDelay() で変えられる(StatusEffect)
    */
    public abstract class ALDamageOverTimeStatusEffectBase : ALMoveRestrictStatusEffectBase
    {
        // 1 回のダメージ
        protected virtual float GetTickDamage() => AppliedParams.Param2.Float;

        protected override void OnTick()
        {
            Actor.EventBus.Publish(ALActorEvents.RequestDamage, ALDamageInfo.Continuous(GetTickDamage()));
        }
    }
}
#nullable restore
