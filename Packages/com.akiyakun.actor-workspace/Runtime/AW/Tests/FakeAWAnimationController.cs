using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ActorWorkspace.Tests
{
    public class FakeAWAnimationController : IAWAnimationController
    {
        IAWTrack track = default;

        // public IReadOnlyList<IAWAnimation> AnimationList => new List<IAWAnimation> { new FakeAWAnimation() };

        public event System.Action<IAWAnimation> OnAnimationComplate;
        public event System.Action<IAWAnimation, AWEventData> OnAnimationEvent;

        public IList<IAWAnimation> GetAnimationList() => animations.ToList<IAWAnimation>();
        public IAWAnimation? GetAnimation(string name) => animations.Find(a => a.Name == name);

        List<FakeAWAnimation> animations = new List<FakeAWAnimation>
        {
            new FakeAWAnimation("TestAnimation1"),
            new FakeAWAnimation("TestAnimation2"),
            new FakeAWAnimation("TestAnimation3")
        };

        // public FakeAWAnimationController()
        // {
        // }

        public void DoUpdate(float deltaTime)
        {
            // foreach (var track in trackList)
            for (int i = 0; i < IAWTrack.MaxTrack; i++)
            {
                // GetTrack(i).Animation?.DoUpdate(deltaTime);
            }
        }

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
