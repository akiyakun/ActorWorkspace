#nullable enable
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ActorWorkspace.Tests
{
    public class StubAWActorFactory : IAWActorFactory
    {
        public event System.Action<IAWActor> OnCreated = null!;
        public event System.Action<IAWActor> OnRelease = null!;

        AWActorContextProvider contextProvider = null!;

        private StubAWActorFactory() {}

        public StubAWActorFactory(AWActorContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
            Debug.Assert(contextProvider != null);
        }

        public UniTask<IAWActor?> CreateAsync(ActorCreateParam param, CancellationToken cancellationToken = default)
        {
            var actor = new StubAWActor(contextProvider, param.Id, param.Category);
            OnCreated?.Invoke(actor);
            return UniTask.FromResult<IAWActor?>(actor);
        }

        public bool Release(IAWActor actor)
        {
            OnRelease?.Invoke(actor);
            return true;
        }
    }
}
#nullable restore