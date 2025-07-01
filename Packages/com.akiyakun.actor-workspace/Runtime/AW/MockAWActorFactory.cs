using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ActorWorkspace
{
    public class MockAWActorFactory : IAWActorFactory
    {
        public UniTask<IAWActor> CreateAsync(int id, CancellationToken cancellationToken = default)
        {
            return UniTask.FromResult<IAWActor>(new MockAWActor());
        }
    }
}
