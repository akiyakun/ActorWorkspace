using UnityEngine;
using afl;

namespace ActorWorkspace.InAppDebug
{
    // 現在ビューワーで読み込んでいるアクターの情報
    public class WorkingActorContext
    {
        // public GameObject GameObject;
        public IAWActor Actor;

        public void Release()
        {
            // FIXME:
            // if (GameObject != null)
            // {
            //     Object.DestroyImmediate(GameObject);
            //     GameObject = null;
            // }
            if (Actor.GameObject != null)
            {
                Object.DestroyImmediate(Actor.GameObject);
                Actor = null;
            }
        }
    }
}