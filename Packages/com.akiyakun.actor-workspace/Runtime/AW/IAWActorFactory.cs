using System.Runtime.CompilerServices;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ActorWorkspace
{
    public interface IAWActorFactory
    {
        public event System.Action<IAWActor> OnCreated;
        public event System.Action<IAWActor> OnRelease;

        public UniTask<IAWActor> CreateAsync(int id, int category = 0, CancellationToken cancellationToken = default);

        public void Release(IAWActor actor);

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static async UniTask<T> CreateAsync<T>(IAWActorFactory self, int id, int category = 0, CancellationToken cancellationToken = default)
        //     where T : class, IAWActor
        // {
        //     return await self.CreateAsync(id, 0, cancellationToken) as T;
        // }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static async UniTask<IAWActor> CreateAsync(IAWActorFactory self, int id, CancellationToken cancellationToken)
        // {
        //     return await self.CreateAsync(id, 0, cancellationToken);
        // }
    }
}
