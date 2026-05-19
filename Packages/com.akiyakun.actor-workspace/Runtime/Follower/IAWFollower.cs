#nullable enable
namespace ActorWorkspace
{
    public interface IAWFollower
    {
        // アクティブ状態が変化される直前に呼ばれる
        public event System.Action<IAWFollower, bool>? OnActiveChange;
    }
}
#nullable restore