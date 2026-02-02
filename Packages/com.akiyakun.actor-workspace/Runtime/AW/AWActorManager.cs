#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public class AWActorManager : IAWActorManager
    {
        protected IAWActorFactory actorFactory;
        protected UpdateElementManager<IAWActor> updateElementManager;

        // protected LinkedList<IAWActor> actorList = new LinkedList<IAWActor>();

#nullable disable
        private AWActorManager() { }
#nullable restore

        public AWActorManager(IAWActorFactory awActorFactory)
        {
            actorFactory = awActorFactory;
            Debug.Assert(awActorFactory != null);

            updateElementManager = new UpdateElementManager<IAWActor>(enablePrioritySort: true);

            {
                actorFactory.OnCreated += OnCreatedFromFactory;
                actorFactory.OnRelease += OnReleaseFromFactory;

                updateElementManager.OnRemoveElement += OnRemoveElement;
            }
        }

        public virtual void Dispose()
        {
            // 内部でOnRemoveElement()が呼ばれる
            updateElementManager.ReleaseAll();

            {
                actorFactory.OnCreated -= OnCreatedFromFactory;
                actorFactory.OnRelease -= OnReleaseFromFactory;

                updateElementManager.OnRemoveElement -= OnRemoveElement;
            }
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
        public virtual void DoFixedUpdate()
        {
            updateElementManager.DoFixedUpdate();
        }

        // From IAWActorManager
        public virtual bool Add(IAWActor actor)
        {
            D.Log(DefaultLogMask.Lifecycle, $"AWActorManager.Add(): {actor.GetType().Name}");
            return updateElementManager.Add(actor);
        }

        // From IAWActorManager
        public virtual bool Remove(IAWActor actor)
        {
            D.Log(DefaultLogMask.Lifecycle, $"AWActorManager.Remove(): {actor.GetType().Name}");
            return updateElementManager.Remove(actor);
        }


        protected virtual void OnCreatedFromFactory(IAWActor actor)
        {
            D.Log(DefaultLogMask.Lifecycle, $"AWActorManager.OnCreatedActor(): {actor.GetType().Name}");
            updateElementManager.Add(actor);
        }

        protected virtual void OnReleaseFromFactory(IAWActor actor)
        {
            D.Log(DefaultLogMask.Lifecycle, $"AWActorManager.OnRelease(): {actor.GetType().Name}");
            // updateElementManager.Remove(actor);
        }

        protected virtual void OnRemoveElement(IAWActor element)
        {
            D.Log(DefaultLogMask.Lifecycle, $"AWActorManager.OnRemoveElement(): {element.GetType().Name}");
            element.GameObject.SetActive(false);
            actorFactory.Release(element);
        }


    }
}
#nullable restore