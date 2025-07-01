using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ActorWorkspace
{
    public interface IAWActorFactory
    {
        UniTask<IAWActor> CreateAsync(int id, CancellationToken cancellationToken = default);
    }
}
