using UnityEngine;
using afl;

namespace ActorWorkspace.InAppDebug
{
    // 現在ビューワーで読み込んでいるアクターの情報
    public class WorkingActorContext
    {
        // public GameObject GameObject;
        public IAWActor Actor;

        public bool IsValid => Actor != null;

        // public void Reset()
        // {
        //     Actor = null;
        // }

        public void Set(IAWActor actor)
        {
            Debug.Assert(actor != null);
            Actor = actor;
        }

        public void Reset()
        {
            Actor = null;
        //     // FIXME:
        //     // if (GameObject != null)
        //     // {
        //     //     Object.DestroyImmediate(GameObject);
        //     //     GameObject = null;
        //     // }
        //     if (Actor.GameObject != null)
        //     {
        //         Object.DestroyImmediate(Actor.GameObject);
        //         Actor = null;
        //     }
        }
    }
}