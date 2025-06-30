using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // MEMO:
    // Animatorや、AnimatinClip、SkeletonAnimationなどのアニメーション実装クラスをラップする
    public interface IAWAnimation
    {
        public string Name { get; }

        // public IEnumerable<AWAnimationData> AnimationList { get; }
        // public IReadOnlyList<AWAnimationData> AnimationList { get; }

    }
}
