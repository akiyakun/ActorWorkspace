using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ActorWorkspace
{
    // アクターの表示部分(View)
    public interface IAWActorDisplay
    {
        // public GameObject GameObject { get; }

        // public UniTask<int> InitializeAsync(IAWActor awActor, CancellationToken cancellationToken);

        public void Restore();
        public void DoUpdate(float deltaTime);

    }
}
