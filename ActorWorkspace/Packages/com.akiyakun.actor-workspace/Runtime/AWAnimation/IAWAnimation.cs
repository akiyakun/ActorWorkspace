using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // MEMO:
    // IAWAnimationが保持するそれぞれのアニメーションのデータ
    public class AWAnimationData
    {
        public string Name;
    }

    // MEMO:
    // Animatorや、AnimatinClip、SkeletonAnimationなどのアニメーション実装クラスをラップする
    public interface IAWAnimation
    {
        public IEnumerable<AWAnimationData> AnimationList { get; }

    }
}
