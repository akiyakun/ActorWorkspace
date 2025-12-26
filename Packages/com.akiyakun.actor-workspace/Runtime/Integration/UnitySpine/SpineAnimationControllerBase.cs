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
        IAWExtraData? IAWAnimationController.ExtraData => ExtraData;
        public SpineExtraDataScriptableObject? ExtraData { get; protected set; }
        public abstract bool IsVisibility { get; set; }

        public virtual bool EnableRootMotion { get; set; }
        public virtual bool ApplyRootMotionPositionX { get; set; }
        public virtual bool ApplyRootMotionPositionY { get; set; }


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

        public IAWAnimation? GetAnimation(int nameHash)
        {
            if (animationHashMap.TryGetValue(nameHash, out TAnimation value)) return value;
            Debug.LogWarning($"GetAnimation: Not found nameHash={nameHash}");
            return null;
        }

        public IAWAnimation? GetAnimation(string name)
        {
            if (animationHashMap.TryGetValue(Utility.StringToHashId(name), out TAnimation value)) return value;
            Debug.LogWarning($"GetAnimation: Not found name={name}");
            return null;
        }

        protected TAnimation? GetAnimationImpl(int nameHash)
        {
            if (animationHashMap.TryGetValue(nameHash, out TAnimation value)) return value;
            Debug.LogWarning($"GetAnimation: Not found nameHash={nameHash}");
            return null;
        }


        public abstract void SetEmptyAnimation(AWAnimationOption option = default);

        public abstract IAWTrack? SetAnimation(int nameHash, AWAnimationOption option = default);
        public IAWTrack? SetAnimation(string name, AWAnimationOption option = default) => SetAnimation(Utility.StringToHashId(name), option);

        // FIXME; spine
        // public abstract void SetEmptyAnimation(int trackIndex, float mixDuration = -1.0f);
        // public abstract IAWTrack? SetAnimation(int trackIndex, IAWAnimation animation, bool loop);
        // public abstract void AddAnimation(int trackIndex, IAWAnimation animation, bool loop, float delay = 0.0f);
        // fixme: loop intにしたい

        public abstract IAWAnimation? GetCurrentAnimation(int track = 0);

        public abstract bool IsPlayingAnimation(int nameHash, int track = 0);
        public bool IsPlayingAnimation(string name, int track = 0) => IsPlayingAnimation(Utility.StringToHashId(name), track);

        public abstract IAWTrack? GetTrack(int trackIndex);


        #region ExtraData
        Dictionary<string, GameObject> followObjectDictionary = new();

        protected void CreateBoneFollowers(Transform parent, SkeletonRenderer skeletonRenderer)
        {
            if (ExtraData == null) return;

            var nameList = ExtraData.GetAttachmentNames(SpineExtraDataScriptableObject.EffectBoneFollower);
            if (nameList == null) return;

            foreach (var name in nameList)
            {
                // var newObject = new GameObject($"BoneFollower_{name}");
                var newObject = new GameObject(name);// 取得したいときにイベント名と同名の方が都合が良い
                newObject.transform.SetParent(parent, worldPositionStays: false);
                newObject.transform.ResetLocalTransform();

                var follower = newObject.AddComponent<BoneFollower>();
                follower.skeletonRenderer = skeletonRenderer;
                follower.Initialize();
                // Debug.Log($"Add BoneFollower: boneName={name}");
                follower.SetBone(name);

                // MEMO: boneNameに設定だとうまく動かない
                // https://zenn.dev/happy_elements/articles/a9bbe3c99aefc5
                // follower.boneName = name;

                AddFollowObject(name, newObject);
            }
        }

        protected void CreatePointFollowers(Transform parent, SkeletonRenderer skeletonRenderer)
        {
            if (ExtraData == null) return;

            var nameList = ExtraData.GetAttachmentNames(SpineExtraDataScriptableObject.EffectPointFollower);
            if (nameList == null) return;

            foreach (var name in nameList)
            {
                // var newObject = new GameObject($"BoneFollower_{name}");
                var newObject = new GameObject(name);// 取得したいときにイベント名と同名の方が都合が良い
                newObject.transform.SetParent(parent, worldPositionStays: false);
                newObject.transform.ResetLocalTransform();

                var follower = newObject.AddComponent<PointFollower>();
                follower.skeletonRenderer = skeletonRenderer;
                follower.Initialize();
                // Debug.Log($"Add PointFollower: slotName={name}");
                follower.slotName = name;

                AddFollowObject(name, newObject);
            }
        }

        public GameObject? GetFollowObject(string name)
        {
            if (followObjectDictionary.TryGetValue(name, out var obj))
            {
                return obj;
            }

            return null;
        }

        protected void AddFollowObject(string name, GameObject obj)
        {
            followObjectDictionary.Add(name, obj);
        }


        protected void CreateFolders(Transform parent, SkeletonRenderer skeletonRenderer, string key)
        {
            if (ExtraData == null) return;

            var nameList = ExtraData.GetAttachmentNames(key);
            if (nameList == null) return;

            foreach (var name in nameList)
            {
                // var newObject = new GameObject($"BoneFollower_{name}");
                var newObject = new GameObject(name);// 取得したいときにイベント名と同名の方が都合が良い
                newObject.transform.SetParent(parent, worldPositionStays: false);
                newObject.transform.ResetLocalTransform();

                var follower = newObject.AddComponent<BoundingBoxFollower>();
                follower.skeletonRenderer = skeletonRenderer;
                follower.Initialize();
                // Debug.Log($"Add PointFollower: slotName={name}");
                follower.slotName = name;
                follower.isTrigger = true;

                AddFollowObject(name, newObject);
            }
        }

        #endregion
    }
}
#nullable restore