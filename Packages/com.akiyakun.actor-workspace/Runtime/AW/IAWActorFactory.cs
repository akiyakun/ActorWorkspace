#nullable enable
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ActorWorkspace
{
    // Actorの生成と解放
    // Actorの表示状態や、プール管理などは担当しない
    public interface IAWActorFactory// : System.IDisposable
    {
        public event System.Action<IAWActor> OnCreated;
        public event System.Action<IAWActor> OnRelease;

        // 非表示状態で生成されます
        public UniTask<IAWActor?> CreateAsync(ActorCreateParam param, CancellationToken cancellationToken = default);

        public bool Release(IAWActor actor);

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
#nullable restore