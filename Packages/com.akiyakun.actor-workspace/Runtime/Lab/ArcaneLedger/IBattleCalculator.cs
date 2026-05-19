#nullable enable
namespace afl.ArcaneLedger
{
    // MEMO:
    // BattleSystem だと仰々しいので BattleCalculator に
    public interface IBattleCalculator
    {
        public EventBus<string> EventBus { get; }
    }
}
#nullable restore