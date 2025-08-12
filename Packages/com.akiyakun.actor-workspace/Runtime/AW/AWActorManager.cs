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
        private AWActorManager() {}
        #nullable enable

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

        // From IAWActorManager
        public virtual void DoUpdate(float deltaTime)
        {
            updateElementManager.DoUpdate(deltaTime);
        }

        // From IAWActorManager
        public virtual void DoFixedUpdate()
        {
            updateElementManager.DoFixedUpdate();
        }

        // From IAWActorManager
        public virtual bool Add(IAWActor actor)
        {
            Debug.Log($"AWActorManager.Add(): {actor.GetType().Name}");
            return updateElementManager.Add(actor);
        }

        // From IAWActorManager
        public virtual bool Remove(IAWActor actor)
        {
            Debug.Log($"AWActorManager.Remove(): {actor.GetType().Name}");
            return updateElementManager.Remove(actor);
        }


        protected virtual void OnCreatedFromFactory(IAWActor actor)
        {
            Debug.Log($"AWActorManager.OnCreatedActor(): {actor.GetType().Name}");
            updateElementManager.Add(actor);
        }

        protected virtual void OnReleaseFromFactory(IAWActor actor)
        {
            Debug.Log($"AWActorManager.OnRelease(): {actor.GetType().Name}");
            // updateElementManager.Remove(actor);
        }

        protected virtual void OnRemoveElement(IAWActor element)
        {
            Debug.Log($"AWActorManager.OnRemoveElement(): {element.GetType().Name}");
            element.GameObject.SetActive(false);
            actorFactory.Release(element);
        }


    }
}
#nullable restore