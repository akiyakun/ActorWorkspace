using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ActorWorkspace
{
    public interface IAWActorFactory
    {
        public event System.Action<IAWActor> OnCreated;

        public UniTask<IAWActor> CreateAsync(int id, CancellationToken cancellationToken = default);
    }
}
