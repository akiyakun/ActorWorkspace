using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // fixme: interface
    public class IAWTrack
    {
        public static int MaxTrack = 4;

        public virtual float TimeScale { get; set; }
        public virtual float MixDuration { get; set; }

        public virtual IAWAnimation Animation { get; protected set; }
    }

    // MEMO:
    // キャラクターの一般的な操作を提供
    public interface IAWAnimationController
    {
        public IReadOnlyList<IAWAnimation> AnimationList { get; }

        // public System.Action<AWAnimationData> OnAnimationComplate { get; }
        public event System.Action<IAWAnimation> OnAnimationComplate;


        // FIXME; spine
        public void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f);
        public IAWTrack SetAnimation(int trackIndex, IAWAnimation animation, bool loop);
        public void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f);
        // fixme: loop intにしたい

        public IAWTrack GetTrack(int trackIndex);
    }
}
