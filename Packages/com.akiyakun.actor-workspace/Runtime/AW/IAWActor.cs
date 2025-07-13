using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    public interface IAWActor
    {
        public GameObject GameObject { get; }
        // public IAWAnimation Animation { get; }
        public IAWAnimationController AnimationController { get; }
        public IReadOnlyList<IAWSkin> SkinList { get; }

        public void SetSkin(int skinIndex);
    }
}
