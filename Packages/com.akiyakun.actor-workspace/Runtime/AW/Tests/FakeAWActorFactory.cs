using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ActorWorkspace.Tests
{
    public class FakeAWActorFactory : IAWActorFactory
    {
        public event System.Action<IAWActor> OnCreated;
        public event System.Action<IAWActor> OnRelease;

        AWActorContextProvider contextProvider;

        private FakeAWActorFactory() {}

        public FakeAWActorFactory(AWActorContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
            Debug.Assert(contextProvider != null);
        }

        public UniTask<IAWActor> CreateAsync(int id, int category, CancellationToken cancellationToken = default)
        {
            var actor = new FakeAWActor(contextProvider, category);
            OnCreated?.Invoke(actor);
            return UniTask.FromResult<IAWActor>(actor);
        }

        public void Release(IAWActor actor)
        {
            OnRelease?.Invoke(actor);
        }
    }
}
