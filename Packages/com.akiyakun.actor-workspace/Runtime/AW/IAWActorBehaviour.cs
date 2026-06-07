#nullable enable
using afl;

namespace ActorWorkspace
{
    // MEMO: インターフェースはいらんかも
    public interface IAWActorBehaviour :
        // IAsyncInitializable,
        IUpdateElement
    {
        public bool IsActive { get; }
        public IAWActor Actor { get; }
        // public string Name { get; }
        public EventBus<string> EventBus { get; }
        public VariableTable Variables { get; }
        public EventBag EventBag { get; }

        public void Initialize(IAWActor actor);
        public void Terminate();

        // 初期状態に戻す
        public void Restore();

        public void Awake();
        public void Start();
        public void Destroy();

    }
}
#nullable restore