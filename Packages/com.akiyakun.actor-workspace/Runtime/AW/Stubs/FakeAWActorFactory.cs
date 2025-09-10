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

        public UniTask<IAWActor> CreateAsync(ActorCreateParam param, CancellationToken cancellationToken = default)
        {
            var actor = new FakeAWActor(contextProvider, param.Id, param.Category);
            OnCreated?.Invoke(actor);
            return UniTask.FromResult<IAWActor>(actor);
        }

        public bool Release(IAWActor actor)
        {
            OnRelease?.Invoke(actor);
            return true;
        }
    }
}
