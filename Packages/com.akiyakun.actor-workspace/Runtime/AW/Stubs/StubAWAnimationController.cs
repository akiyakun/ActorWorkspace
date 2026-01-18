#nullable enable
using System.Collections.Generic;
using System.Linq;
using afl;
using UnityEngine;

namespace ActorWorkspace.Tests
{
    public class StubAWAnimationController : IAWAnimationController
    {
        IAWTrack? track = null;

        // public IReadOnlyList<IAWAnimation> AnimationList => new List<IAWAnimation> { new FakeAWAnimation() };

        public event System.Action<IAWAnimation> OnAnimationEntered = null!;
        public event System.Action<IAWAnimation> OnAnimationComplete = null!;
        public event System.Action<IAWAnimation, AWAnimationEventData> OnAnimationEvent = null!;

        public IAWAnimationParameter AnimationParameter { get; protected set; } = new AWAnimationParameter();
        public IAWExtraData? ExtraData { get; protected set; }
        public bool IsVisibility { get; set; } = true;
        public bool EnableRootMotion { get; set; } = false;
        public bool ApplyRootMotionPositionX { get; set; }
        public bool ApplyRootMotionPositionY { get; set; }

        public IReadOnlyList<IAWAnimation> GetAnimationList() => animations.ToList<IAWAnimation>();
        public IAWAnimation? GetAnimation(int nameHash) => animations.Find(a => Utility.StringToHashId(a.Name) == nameHash);
        public IAWAnimation? GetAnimation(string name) => animations.Find(a => a.Name == name);

        List<StubAWAnimation> animations = new List<StubAWAnimation>
        {
            new StubAWAnimation("TestAnimation1"),
            new StubAWAnimation("TestAnimation2"),
            new StubAWAnimation("TestAnimation3")
        };

        // public FakeAWAnimationController()
        // {
        // }

        public void Dispose()
        {
        }

        public void Restore()
        {
        }

        public void DoUpdate(float deltaTime)
        {
            // foreach (var track in trackList)
            for (int i = 0; i < IAWTrack.MaxTrack; i++)
            {
                // GetTrack(i).Animation?.DoUpdate(deltaTime);
            }
        }


        public void SetEmptyAnimation(AWAnimationOption option = default)
        {
        }

        public IAWTrack? SetAnimation(int nameHash, AWAnimationOption option = default)
        {
            return track;
        }

        public IAWTrack? SetAnimation(string name, AWAnimationOption option = default)
        {
            return track;
        }

        // public IAWTrack? SetAnimation(int hashId, bool loop, int trackNum = 0)
        // {
        //     return track;
        // }

        // // FIXME; spine
        // public IAWTrack? SetAnimation(int trackIndex, IAWAnimation animation, bool loop)
        // {
        //     return track;
        // }
        // public void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f)
        // {
        // }

        public IAWAnimation? GetCurrentAnimation(int track = 0)
        {
            return null;
        }

        public bool IsPlayingAnimation(int nameHash, int track = 0)
        {
            return false;
        }

        public bool IsPlayingAnimation(string name, int track = 0)
        {
            return false;
        }

        public IAWTrack? GetTrack(int trackIndex)
        {
            return track;
        }
    }
}
#nullable restore