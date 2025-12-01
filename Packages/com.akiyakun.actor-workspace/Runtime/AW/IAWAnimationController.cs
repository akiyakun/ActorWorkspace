#nullable enable
using System.Runtime.CompilerServices;
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

        public IReadOnlyList<IAWAnimation> GetAnimationList();
        public IAWAnimation? GetAnimation(int hashId);
        public IAWAnimation? GetAnimation(string name);

        public void SetEmptyAnimation(AWAnimationOption option = default);
        // public void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f);
        // public void SetDefaultAnimation(int hashId, bool loop, int trackNum = 0);

        public IAWTrack? SetAnimation(int hashId, AWAnimationOption option = default);

        // FIXME; spine
        // public void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f);
        // fixme: loop intにしたい

        public IAWAnimation? GetCurrentAnimation(int track = 0);
        public bool IsPlayingAnimation(int hashId, int track = 0);

        public IAWTrack? GetTrack(int trackIndex);

        public bool IsVisibility { get; set; }

    }

    public static class IAWAnimationControllerExtensionMethods
    {
#region ExtensionMethods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEmptyAnimation(this IAWAnimationController self, int track)
        {
            self.SetEmptyAnimation(new AWAnimationOption { Track = track });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAWTrack? SetAnimation(this IAWAnimationController self, int hashId, bool loop)
        {
            return self.SetAnimation(hashId, new AWAnimationOption { Flags = loop ? (uint)AWAnimationOptionFlag.Loop : 0 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAWTrack? SetAnimation(this IAWAnimationController self, int hashId, bool loop, bool immediate)
        {
            return self.SetAnimation(hashId, new AWAnimationOption
            {
                Flags = (uint)(
                    (loop ? AWAnimationOptionFlag.Loop : 0)
                    | (immediate ? AWAnimationOptionFlag.Immediate : 0)
                )
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAWTrack? SetAnimation(this IAWAnimationController self, int hashId, bool loop, int track)
        {
            return self.SetAnimation(hashId, new AWAnimationOption { Track = track, Flags = loop ? (uint)AWAnimationOptionFlag.Loop : 0 });
        }
#endregion
    }
}
#nullable restore