#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    public interface IAWAnimationController : System.IDisposable
    {
        // public IReadOnlyList<IAWAnimation> AnimationList { get; }

        #region Events
        // アニメーションが開始されたときにコールバックされます
        public event System.Action<IAWAnimation> OnAnimationEntered;

        // アニメーションが完了したときにコールバックされます
        public event System.Action<IAWAnimation> OnAnimationComplate;

        // アニメーションのタイムラインに含まれるイベントが発生したときにコールバックされます
        public event System.Action<IAWAnimation, AWAnimationEventData> OnAnimationEvent;
        #endregion

        public void Restore();

        public void DoUpdate(float deltaTime);

        public IAWAnimationParameter AnimationParameter { get; }
        public IAWExtraData? ExtraData { get; }

        public IReadOnlyList<IAWAnimation> GetAnimationList();
        public IAWAnimation? GetAnimation(int nameHash);
        public IAWAnimation? GetAnimation(string name);

        public void SetEmptyAnimation(AWAnimationOption option = default);
        // public void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f);
        // public void SetDefaultAnimation(int hashId, bool loop, int trackNum = 0);

        public IAWTrack? SetAnimation(int nameHash, AWAnimationOption option = default);
        public IAWTrack? SetAnimation(string name, AWAnimationOption option = default);

        // FIXME; spine
        // public void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f);
        // fixme: loop intにしたい

        public IAWAnimation? GetCurrentAnimation(int track = 0);

        public bool IsPlayingAnimation(int nameHash, int track = 0);
        public bool IsPlayingAnimation(string name, int track = 0);

        public IAWTrack? GetTrack(int trackIndex);

        public bool IsVisibility { get; set; }


        #region RootMotion
        public bool EnableRootMotion { get; set; }
        public bool ApplyRootMotionPositionX { get; set; }
        public bool ApplyRootMotionPositionY { get; set; }
        #endregion
    }
}
#nullable restore