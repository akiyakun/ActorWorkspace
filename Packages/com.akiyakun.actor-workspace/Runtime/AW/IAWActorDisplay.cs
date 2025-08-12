using System.Threading;
using Cysharp.Threading.Tasks;

namespace ActorWorkspace
{
    // アクターの表示部分(View)
    public interface IAWActorDisplay
    {
        // public UniTask<int> InitializeAsync(IAWActor awActor, CancellationToken cancellationToken);

        public void Restore();
        public void DoUpdate(float deltaTime);
    }
}
