#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public class AWActorManager<TActor> : IAWActorManager
        where TActor : class, IAWActor
    {
        public IReadOnlyList<TActor> ActorList => updateElementManager.ReadOnlyList;

        bool showErrorOnAutoCreate;

        protected IAWActorFactory actorFactory;
        protected UpdateElementManager<TActor> updateElementManager;

        class CategoryPool
        {
            public Dictionary<int, Stack<TActor>> ActorPools = new();
        }
        Dictionary<int, CategoryPool> categoryPools = new();

        GameObject gameObject;
        EventBag eventBag = new();

#nullable disable
        private AWActorManager() { }
#nullable enable

        public AWActorManager(IAWActorFactory awActorFactory)
        {
            actorFactory = awActorFactory;
            Debug.Assert(awActorFactory != null);

            updateElementManager = new UpdateElementManager<TActor>(enablePrioritySort: true);

            // イベントの購読
            {
                // actorFactory.OnCreated += OnCreatedFromFactory;
                // actorFactory.OnRelease += OnReleaseFromFactory;

                eventBag.In(updateElementManager,
                    (entity) => entity.OnRemoveElement += OnRemoveElement,
                    (entity) => entity.OnRemoveElement -= OnRemoveElement);
            }

            gameObject = new GameObject("AWActorManager");
            Object.DontDestroyOnLoad(gameObject);
        }

        public virtual void Dispose()
        {
            // 内部でOnRemoveElement()が呼ばれる
            updateElementManager.ReleaseAll();

            // 全プールの解放
            {
                foreach (var categoryPool in categoryPools)
                {
                    foreach (var actorPool in categoryPool.Value.ActorPools)
                    {
                        ClearPool(actorPool.Key, categoryPool.Key);
                    }
                }
                categoryPools.Clear();
            }

            eventBag.Dispose();
            Object.Destroy(gameObject);
        }

        // From IAWActorManager
        public virtual void DoUpdate(float deltaTime)
        {
            updateElementManager.DoUpdate(deltaTime);
        }

        // From IAWActorManager
        public virtual void DoLateUpdate(float deltaTime)
        {
            updateElementManager.DoLateUpdate(deltaTime);
        }

        // From IAWActorManager
        public virtual void DoFixedUpdate(float deltaTime)
        {
            updateElementManager.DoFixedUpdate(deltaTime);
        }

        // From IAWActorManager
        public virtual int GetPoolCount(int id, int category)
        {
            // if (pools.TryGetValue(id, out var stack)) return stack.Count;
            if (categoryPools.TryGetValue(category, out var categoryPool))
            {
                if (categoryPool.ActorPools.TryGetValue(id, out var stack))
                {
                    return stack.Count;
                }
            }
            return 0;
        }

        // From IAWActorManager
        public virtual void ClearPool(int id, int category)
        {
            if (categoryPools.TryGetValue(category, out var categoryPool) == false) return;
            if (categoryPool.ActorPools.TryGetValue(id, out var stack) == false) return;

            while (stack.Count > 0)
            {
                var actor = stack.Pop();
                actorFactory.Release(actor);
            }
        }

        // From IAWActorManager
        public virtual async UniTask AddToPoolAsync(int id, int category, int count, CancellationToken cancellationToken = default)
        {
            if (categoryPools.TryGetValue(category, out var categoryPool) == false)
            {
                categoryPool = new CategoryPool();
                categoryPools.Add(category, categoryPool);
            }

            if (categoryPool.ActorPools.TryGetValue(id, out var stack) == false)
            {
                stack = new Stack<TActor>();
                categoryPool.ActorPools.Add(id, stack);
            }

            for (int i = 0; i < count; i++)
            {
                var actor = await actorFactory.CreateAsync(new ActorCreateParam(id, category), cancellationToken);
                if (actor == null)
                {
                    throw new System.Exception($"AWActorManager.AddToPool(): Failed to create actor. id={id}");
                }
                stack.Push((TActor)actor);

                actor.GameObject.SetActive(false);
                actor.GameObject.transform.SetParent(gameObject.transform);
                // actor.GameObject.hideFlags = HideFlags.HideInHierarchy;
            }
        }

        // From IAWActorManager
        // public virtual void AddToPool(int id, int category)
        // {
        //     if (categoryPools.TryGetValue(category, out var categoryPool) == false)
        //     {
        //         categoryPool = new CategoryPool();
        //         categoryPools.Add(category, categoryPool);
        //     }

        //     if (categoryPool.ActorPools.TryGetValue(id, out var stack) == false)
        //     {
        //         stack = new Stack<TActor>();
        //         categoryPool.ActorPools.Add(id, stack);
        //     }

        //     // for (int i = 0; i < count; i++)
        //     {
        //         var actor = actorFactory.Create(new ActorCreateParam(id, category));
        //         if (actor == null)
        //         {
        //             throw new System.Exception($"AWActorManager.AddToPool(): Failed to create actor. id={id}");
        //         }
        //         stack.Push((TActor)actor);

        //         actor.GameObject.transform.SetParent(gameObject.transform);
        //         // actor.GameObject.hideFlags = HideFlags.HideInHierarchy;
        //     }
        // }

        // From IAWActorManager
        public virtual IAWActor? Spawn(int id, int category, GameObject? parent = null, bool autoCreate = false)
        {
            Stack<TActor>? stack = null;
            bool addToPool = false;

            if (categoryPools.TryGetValue(category, out var categoryPool) == false) addToPool = true;
            else if (categoryPool.ActorPools.TryGetValue(id, out stack) == false) addToPool = true;
            else if (stack.Count == 0) addToPool = true;

            // プールが空で自動生成しない場合はnullを返す
            if (addToPool && autoCreate == false) return null;

            if (addToPool)
            {
                if (showErrorOnAutoCreate)
                {
                    Debug.LogError($"AWActorManager.Spawn(): Auto creating pool. id={id}, category={category}");
                }

                // 同期的にプールを1つ追加
                // AddToPool(id, category);
                return Spawn(id, category, parent, autoCreate: false);
            }

            if (stack == null) return null;

            var actor = stack.Pop();
            actor.Restore();

            // UnityEngine.SceneManagement.SceneManager.GetActiveScene().
            if (parent != null) actor.GameObject.transform.SetParent(parent.transform);

            if (updateElementManager.Add(actor) == false)
            {
                throw new System.Exception($"AWActorManager.Spawn(): Failed to add actor to manager. id={id}");
            }

            return actor;
        }

        public virtual void Despawn(IAWActor actor)
        {
            if (actor == null) return;
            actor.ElementActive = false;
        }

        /// <summary>
        /// IReadOnlyListなのでforeachしないよう注意してください。
        /// TActorのリストが欲しい場合はActorListプロパティの方を使用してください。
        /// </summary>
        // From IAWActorManager
        public virtual IReadOnlyList<IAWActor> GetActorList() => updateElementManager.ReadOnlyList;

        public virtual List<IAWActor> GetActorListAtCluster(int cluster)
        {
            var ret = new List<IAWActor>();
            var list = updateElementManager.ReadOnlyList;
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (list[i].ActorParam.Cluster == cluster) ret.Add(list[i]);
            }
            return ret;
        }

        // protected virtual void OnCreatedFromFactory(IAWActor actor)
        // {
        //     D.Log(CoreLogMask.Lifecycle, $"AWActorManager.OnCreatedActor(): {actor.GetType().Name}");
        //     updateElementManager.Add(actor as TActor);
        // }

        // protected virtual void OnReleaseFromFactory(IAWActor actor)
        // {
        //     D.Log(CoreLogMask.Lifecycle, $"AWActorManager.OnRelease(): {actor.GetType().Name}");
        //     // updateElementManager.Remove(actor);
        // }

        protected virtual void OnRemoveElement(IAWActor element)
        {
            D.Log(CoreLogMask.Lifecycle, $"AWActorManager.OnRemoveElement(): {element.GetType().Name}");
            element.GameObject.SetActive(false);
            element.GameObject.transform.SetParent(gameObject.transform);
            actorFactory.Release(element);
        }


    }
}
#nullable restore