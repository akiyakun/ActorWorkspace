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
    public abstract class SpineAnimationController : IAWAnimationController
    {
        public event System.Action<IAWAnimation>? OnAnimationComplate;
        protected virtual void InvokeAnimationComplate(IAWAnimation animation) => OnAnimationComplate?.Invoke(animation);
        public event System.Action<IAWAnimation, AWEventData>? OnAnimationEvent;
        protected virtual void InvokeAnimationEvent(IAWAnimation animation, AWEventData eventData) => OnAnimationEvent?.Invoke(animation, eventData);


        // SkeletonAnimation と SkeletonMecanim 双方が継承しているインターフェース
        protected ISkeletonAnimation skeletonAnimationInterface;

        protected IAWEventDecoder eventDecoder;
        protected Dictionary<string, SpineAnimation> animations = new();
        protected List<SpineTrack> trackList = new(IAWTrack.MaxTrack);


#nullable disable
        protected SpineAnimationController() { }
#nullable enable

        public SpineAnimationController(ISkeletonAnimation skeletonAnimationInterface, IAWEventDecoder eventDecoder)
        {
            this.skeletonAnimationInterface = skeletonAnimationInterface;
            Debug.Assert(skeletonAnimationInterface != null);

            this.eventDecoder = eventDecoder;
            Debug.Assert(eventDecoder != null);

            // トラックの生成
            {
                for (int i = 0; i < IAWTrack.MaxTrack; i++)
                {
                    trackList.Add(new SpineTrack(i));
                }
            }
        }

        public abstract UniTask<int> InitializeAsync(CancellationToken cancellationToken);

        public abstract void Dispose();

        // FIXME:
        public void DoUpdate(float deltaTime)
        {
            // SpineのUpdateは自動でやってくれるので特に何もしない
            // skeletonAnimationInterface.Update(deltaTime);

            // // Trackの更新
            // foreach (var track in trackList)
            // {
            //     track.DoUpdate(deltaTime);
            // }
        }

        public IList<IAWAnimation> GetAnimationList()
        {
            return animations.Values.ToList<IAWAnimation>();
        }

        public IAWAnimation? GetAnimation(string name)
        {
#if __DEBUG__
            if (animations.Get(name) is IAWAnimation animation) return animation;
            Debug.Assert(false, $"GetAnimation: Not found name={name}");
            return null;
#else
            return animations.Get(name);
#endif
            // for (int i = 0; i < animations.Count; i++)
            // {
            //     if (animations[i].Name == name) return animations[i];
            // }
            // return null;
        }

        // FIXME; spine
        public abstract void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f);
        public abstract IAWTrack? SetAnimation(int trackIndex, IAWAnimation animation, bool loop);
        public abstract void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f);
        // fixme: loop intにしたい

        public abstract IAWTrack? GetTrack(int trackIndex);
    }
}
#nullable restore