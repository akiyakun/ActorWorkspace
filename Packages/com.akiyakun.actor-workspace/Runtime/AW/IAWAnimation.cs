#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // アニメーションのデータクラス
    // 再生制御は IAWAnimationController が行う
    //
    // MEMO:
    // Animatorや、AnimatinClip、SkeletonAnimationなどのアニメーション実装クラスをラップする
    public interface IAWAnimation
    {
        public string Name { get; }
        public int NameHash { get; }

        // public IEnumerable<AWAnimationData> AnimationList { get; }
        // public IReadOnlyList<AWAnimationData> AnimationList { get; }

        // public void Stop();

    }
}
#nullable restore