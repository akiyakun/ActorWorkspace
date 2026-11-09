#nullable enable
using ActorWorkspace.ArcaneLedger.ActorBehaviour;

namespace ActorWorkspace.ArcaneLedger
{
    public static class ArcaneLedgerExtensions
    {
        // アクターの ArcaneLedgerBehaviour(台帳)を取得する。付いていなければ null
        public static ArcaneLedgerBehaviour? GetLedger(this IAWActor actor)
        {
            return actor.ActorBehaviourController.Get<ArcaneLedgerBehaviour>();
        }
    }
}
#nullable restore
