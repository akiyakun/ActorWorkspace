#nullable enable
using afl;

namespace ActorWorkspace
{
    // MEMO: インターフェースはいらんかも
    public interface IAWActorBehaviour :
        // IAsyncInitializable,
        IUpdateElement
    {
        // public IAWActor Actor { get; }
        // public string Name { get; }

        public void Initialize(IAWActor actor);

        // 初期状態に戻す
        public void Restore();

        public void DoAwake();
        public void DoDestroy();

    }
}
#nullable restore