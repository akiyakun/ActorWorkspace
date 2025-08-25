#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // MEMO:
    // キャラクターの一般的な操作を提供
    public interface IAWAnimationController
    {
        // public IReadOnlyList<IAWAnimation> AnimationList { get; }

        // public System.Action<AWAnimationData> OnAnimationComplate { get; }
        public event System.Action<IAWAnimation> OnAnimationComplate;
        public event System.Action<IAWAnimation, AWEventData> OnAnimationEvent;
        // public event System.Action<AWEventData> OnEvent;

        public void DoUpdate(float deltaTime);

        public IList<IAWAnimation> GetAnimationList();
        public IAWAnimation? GetAnimation(string name);

        // FIXME; spine
        public void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f);
        public IAWTrack? SetAnimation(int trackIndex, IAWAnimation animation, bool loop);
        public void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f);
        // fixme: loop intにしたい

        public IAWTrack? GetTrack(int trackIndex);
    }
}
#nullable restore