#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Spine.Unity;
using System.Linq;
using afl;

namespace ActorWorkspace.UnitySpine
{
    // 対になるSpineのクラスは SkeletonAnimation と SkeletonMecanim クラス。
    // 双方の基底抽象クラスとなります。
    public abstract class SpineAnimationControllerBase<TAnimation, TTrack> : IAWAnimationController
        where TAnimation : class, IAWAnimation
        where TTrack : class, IAWTrack
    {
        #region Events
        public event System.Action<IAWAnimation> OnAnimationEntered = null!;
        protected virtual void InvokeAnimationEntered(IAWAnimation animation) => OnAnimationEntered?.Invoke(animation);
        public event System.Action<IAWAnimation> OnAnimationComplate = null!;
        protected virtual void InvokeAnimationComplate(IAWAnimation animation) => OnAnimationComplate?.Invoke(animation);
        public event System.Action<IAWAnimation, AWAnimationEventData> OnAnimationEvent = null!;
        protected virtual void InvokeAnimationEvent(IAWAnimation animation, AWAnimationEventData eventData) => OnAnimationEvent?.Invoke(animation, eventData);
        #endregion

        public abstract IAWAnimationParameter AnimationParameter { get; protected set; }
        public abstract bool IsVisibility { get; set; }

        // SkeletonAnimation と SkeletonMecanim 双方が継承しているインターフェース
        protected ISkeletonAnimation skeletonAnimationInterface;

        // protected Dictionary<string, SpineAnimation> animations = new();
        protected SortedDictionary<int, TAnimation> animationHashMap = new();
        protected List<TTrack> trackList = new(IAWTrack.MaxTrack);


#nullable disable
        protected SpineAnimationControllerBase() { }
#nullable enable

        public SpineAnimationControllerBase(ISkeletonAnimation skeletonAnimationInterface)
        {
            this.skeletonAnimationInterface = skeletonAnimationInterface;
            Debug.Assert(skeletonAnimationInterface != null);

            // トラックの生成
            // {
            //     for (int i = 0; i < IAWTrack.MaxTrack; i++)
            //     {
            //         trackList.Add(new TTrack(i));
            //     }
            // }
            CreateTrack();
        }

        public abstract void CreateTrack();

        public abstract UniTask<int> InitializeAsync(CancellationToken cancellationToken);

        public abstract void Dispose();

        public abstract void Restore();

        // FIXME:
        public virtual void DoUpdate(float deltaTime)
        {
            // SpineのUpdateは自動でやってくれるので特に何もしない
            // skeletonAnimationInterface.Update(deltaTime);

            // // Trackの更新
            // foreach (var track in trackList)
            // {
            //     track.DoUpdate(deltaTime);
            // }
        }


        public IReadOnlyList<IAWAnimation> GetAnimationList()
        {
            return animationHashMap.Values.Cast<IAWAnimation>().ToList();
        }

        public IAWAnimation? GetAnimation(int hashId)
        {
            if (animationHashMap.TryGetValue(hashId, out TAnimation value)) return value;
            Debug.LogWarning($"GetAnimation: Not found hashId={hashId}");
            return null;
        }

        public IAWAnimation? GetAnimation(string name)
        {
            if (animationHashMap.TryGetValue(Utility.StringToHashId(name), out TAnimation value)) return value;
            Debug.LogWarning($"GetAnimation: Not found name={name}");
            return null;
        }

        protected TAnimation? GetAnimationImpl(int hashId)
        {
            if (animationHashMap.TryGetValue(hashId, out TAnimation value)) return value;
            Debug.LogWarning($"GetAnimation: Not found hashId={hashId}");
            return null;
        }


        public abstract void SetEmptyAnimation(AWAnimationOption option = default);

        // public abstract IAWTrack? SetAnimation(int hashId, bool loop = false, int trackNum = 0);
        public abstract IAWTrack? SetAnimation(int hashId, AWAnimationOption option = default);

        // FIXME; spine
        // public abstract void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f);
        // public abstract IAWTrack? SetAnimation(int trackIndex, IAWAnimation animation, bool loop);
        // public abstract void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f);
        // fixme: loop intにしたい

        public abstract IAWAnimation? GetCurrentAnimation(int track = 0);
        public abstract bool IsPlayingAnimation(int hashId, int track = 0);

        public abstract IAWTrack? GetTrack(int trackIndex);
    }
}
#nullable restore