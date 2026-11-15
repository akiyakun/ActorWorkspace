using UnityEngine;

namespace ActorWorkspace.InAppDebug
{
    // 現在ビューワーで読み込んでいるアクターの情報
    public class WorkingActorContext
    {
        public IAWActor Actor;

        public bool IsValid => Actor != null;

        public void Set(IAWActor actor)
        {
            Debug.Assert(actor != null);
            Actor = actor;
        }

        public void Reset()
        {
            Actor = null;
        }
    }
}