using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ActorWorkspace
{
    public class MockAWActorFactory : IAWActorFactory
    {
        public event System.Action<IAWActor> OnCreated;

        public UniTask<IAWActor> CreateAsync(int id, int category, CancellationToken cancellationToken = default)
        {
            return UniTask.FromResult<IAWActor>(new MockAWActor(category));
        }
    }
}
