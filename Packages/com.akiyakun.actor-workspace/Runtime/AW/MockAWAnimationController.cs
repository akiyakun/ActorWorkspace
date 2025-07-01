using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    public class MockAWAnimationController : IAWAnimationController
    {
        IAWTrack track = default;

        public IReadOnlyList<IAWAnimation> AnimationList => new List<IAWAnimation> { new MockAWAnimation() };

        public event System.Action<IAWAnimation> OnAnimationComplate;
        public event System.Action<IAWAnimation, AWEventData> OnAnimationEvent;

        // FIXME; spine
        public void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f)
        {

        }
        public IAWTrack SetAnimation(int trackIndex, IAWAnimation animation, bool loop)
        {
            return track;
        }
        public void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f)
        {
        }

        public IAWTrack GetTrack(int trackIndex)
        {
            return track;
        }
    }
}
