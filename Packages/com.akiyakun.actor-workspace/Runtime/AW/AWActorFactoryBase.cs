#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using afl;
using afl.MasterData;

namespace ActorWorkspace
{
    // 実装時にこのクラスを継承すると手間が省けます
    // 継承は必須ではありません
    public abstract class ActorFactoryBase : IAWActorFactory
    {
        // From IAWActorFactory
        public event System.Action<IAWActor>? OnCreated;
        public event System.Action<IAWActor>? OnRelease;

        protected AssetLoader AssetLoader { get; set; }
        // protected AWActorContextProvider ActorContextProvider { get; set; }
        protected AssetRepositoryCategorize AssetRepositoryCategorize { get; }

#nullable disable
        private ActorFactoryBase() { }
#nullable enable

        // public ActorFactoryBase(AWActorContextProvider awActorContextProvider)
        public ActorFactoryBase(AssetRepositoryCategorize assetRepositoryCategorize)
        {
            AssetLoader = new AssetLoader();

            // ActorContextProvider = awActorContextProvider;
            // Debug.Assert(awActorContextProvider != null);

            AssetRepositoryCategorize = assetRepositoryCategorize;
        }

        // public virtual void Dispose()
        // {
        // }

        public async UniTask<IAWActor?> CreateAsync(ActorCreateParam param, CancellationToken cancellationToken)
        {
            var actor = await InnerCreateAsync(param, cancellationToken);
            if (actor == null) return null;
            OnCreated?.Invoke(actor);
            return actor;
        }

        // public IAWActor? Create(ActorCreateParam param)
        // {
        //     var actor = InnerCreate(param);
        //     if (actor == null) return null;
        //     OnCreated?.Invoke(actor);
        //     return actor;
        // }

        // MEMO: 実装先でOnCreated()の呼び出しをする必要はありません
        protected abstract UniTask<IAWActor?> InnerCreateAsync(ActorCreateParam param, CancellationToken cancellationToken);
        // protected abstract IAWActor? InnerCreate(ActorCreateParam param);

        public bool Release(IAWActor actor)
        {
            if (actor is null)
            {
                Debug.LogWarning("ActorFactoryBase.Release(): actor is null");
                return false;
            }

            OnRelease?.Invoke(actor);
            return InnerRelease(actor);
        }

        // MEMO: 実装先でOnRelease()の呼び出しをする必要はありません
        protected abstract bool InnerRelease(IAWActor actor);

        protected AssetModel? GetAssetModel(int id, int category)
        {
            if (id <= 0)
            {
                Debug.Assert(false, $"ActorFactory.GetAssetModel(): id is invalid id={id}");
                return null;
            }

            var repository = AssetRepositoryCategorize.Get(category);
            if (repository == null)
            {
                Debug.Assert(repository != null, $"ActorFactory.GetAssetModel(): repository is null. category={category}");
                return null;
            }

            var assetModel = repository.GetAssetModel(id);
            if (assetModel == null)
            {
                Debug.Assert(assetModel != null, $"ActorFactory.GetAssetModel(): assetModel is null. id={id}, category={category}");
                return null;
            }

            return assetModel;
        }
    }
}
#nullable restore