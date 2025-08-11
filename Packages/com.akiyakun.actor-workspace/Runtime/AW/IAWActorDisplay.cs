using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // アクターの表示部分(View)
    public interface IAWActorDisplay
    {
        public void Restore();
        public void DoUpdate(float deltaTime);
    }
}
