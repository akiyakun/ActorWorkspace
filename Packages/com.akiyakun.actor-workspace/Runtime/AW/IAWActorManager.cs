#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ActorWorkspace
{
    public interface IAWActorManager : System.IDisposable
    {
        public void DoUpdate(float deltaTime);
		public void DoLateUpdate(float deltaTime);
        public void DoFixedUpdate();

        // public bool Add(IAWActor actor);
        // public bool Remove(IAWActor actor);

        public int GetPoolCount(int id, int category);
        public UniTask AddToPool(int id, int category, int count, CancellationToken cancellationToken = default);
        public void ClearPool(int id, int category);

        public IAWActor? Spawn(int id, int category, GameObject? parent = null, bool autoCreate = true);
        public void Despawn(IAWActor actor);

        // アクティブなActorのリストを取得
        public IReadOnlyList<IAWActor> GetActorList();
    }
}
#nullable restore