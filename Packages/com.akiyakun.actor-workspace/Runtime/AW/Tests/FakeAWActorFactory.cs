using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ActorWorkspace.Tests
{
    public class FakeAWActorFactory : IAWActorFactory
    {
        public event System.Action<IAWActor> OnCreated;
        public event System.Action<IAWActor> OnRelease;

        public UniTask<IAWActor> CreateAsync(int id, int category, CancellationToken cancellationToken = default)
        {
            var actor = new FakeAWActor(category);
            OnCreated?.Invoke(actor);
            return UniTask.FromResult<IAWActor>(actor);
        }

        public void Release(IAWActor actor)
        {
            OnRelease?.Invoke(actor);
        }
    }
}
