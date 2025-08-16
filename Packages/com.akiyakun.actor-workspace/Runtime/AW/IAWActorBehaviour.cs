#nullable enable
using afl;

namespace ActorWorkspace
{
    // MEMO: インターフェースはいらんかも
    public interface IAWActorBehaviour :
        // IAsyncInitializable,
        IUpdateElement
    {
        public IAWActor Actor { get; }
        // public string Name { get; }

        public void DoAwake();

    }
}
#nullable restore