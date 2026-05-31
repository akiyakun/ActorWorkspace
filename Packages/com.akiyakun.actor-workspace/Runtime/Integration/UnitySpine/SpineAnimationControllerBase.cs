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
        // public event System.Action<IAWAnimation, Vector2, float> OnUpdateOverride;
        // protected virtual void InvokeUpdateOverride(IAWAnimation animation, Vector2 translation, float rotation) => OnUpdateOverride?.Invoke(animation, translation, rotation);
        public event IAWAnimationController.UpdateOverrideDelegate OnUpdateOverride = null!;
        protected virtual void InvokeUpdateOverride(IAWAnimationController controller, Vector2 translation, float rotation) => OnUpdateOverride?.Invoke(controller, translation, rotation);
        protected bool IsNullOfUpdateOverride => OnUpdateOverride == null;

        public event System.Action<IAWAnimation> OnAnimationEntered = null!;
        protected virtual void InvokeAnimationEntered(IAWAnimation animation) => OnAnimationEntered?.Invoke(animation);
        public event System.Action<IAWAnimation> OnAnimationComplete = null!;
        protected virtual void InvokeAnimationComplete(IAWAnimation animation) => OnAnimationComplete?.Invoke(animation);
        public event System.Action<IAWAnimation, AWAnimationEventData> OnAnimationEvent = null!;
        protected virtual void InvokeAnimationEvent(IAWAnimation animation, AWAnimationEventData eventData) => OnAnimationEvent?.Invoke(animation, eventData);

        public event System.Action<GameObject, IAWAttachmentInfo> OnCreatedAttachment = null!;
        protected virtual void InvokeCreatedAttachment(GameObject attachmentObject, IAWAttachmentInfo attachmentInfo) => OnCreatedAttachment?.Invoke(attachmentObject, attachmentInfo);

        public event System.Action<IAWAnimationController> OnRootMotionChanged = null!;
        protected virtual void InvokeRootMotionChanged() => OnRootMotionChanged?.Invoke(this);
        #endregion

        public abstract IAWAnimationParameter AnimationParameter { get; protected set; }
        IAWExtraData? IAWAnimationController.ExtraData => ExtraData;
        public SpineExtraDataScriptableObject? ExtraData { get; protected set; }
        public abstract bool IsVisibility { get; set; }
        public abstract bool IsPlaying { get; }

        protected AWRootMotionInfo rootMotionInfo = new AWRootMotionInfo();
        public AWRootMotionInfo RootMotionInfo => rootMotionInfo;
        public virtual bool UseRootMotion { get; set; }
        public virtual bool RootMotionStatus { get; protected set; }
        public virtual bool ApplyRootMotionPositionX { get; set; }
        public virtual bool ApplyRootMotionPositionY { get; set; }
        public virtual bool ApplyRootMotionRotation { get; set; }


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

        public abstract void Stop();

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

        protected TAnimation? GetAnimationImpl(string name)
        {
            if (animationHashMap.TryGetValue(Utility.StringToHashId(name), out TAnimation value)) return value;
            Debug.LogWarning($"GetAnimation: Not found name={name}");
            return null;
        }


        public abstract void SetEmptyAnimation(AWAnimationOption option = default);

        public abstract IAWTrack? SetAnimation(int nameHash, AWAnimationOption option = default);
        public abstract IAWTrack? SetAnimation(string name, AWAnimationOption option = default);
        // public IAWTrack? SetAnimation(string name, AWAnimationOption option = default) => SetAnimation(Utility.StringToHashId(name), option);

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

        // ExtraDataの基本的なセットアップ処理
        protected void SetupForExtraData(Transform parent, SkeletonRenderer skeletonRenderer)
        {
            if (skeletonRenderer == null) throw new System.ArgumentNullException(nameof(skeletonRenderer));

            if (ExtraData == null) return;

            if (ExtraData.IsImportError == true)
            {
                throw new System.Exception($"インポートエラーが発生しています name={skeletonRenderer.name}");
            }

            // Followerオブジェクトの作成
            // CreateBoneFollowers(parent, skeletonRenderer);
            // CreatePointFollowers(parent, skeletonRenderer);

            // Folderの処理
            {
                foreach (var info in ExtraData.Folders)
                {
                    CreateFolders(parent, skeletonRenderer, info);
                }
            }

            {
                rootMotionInfo.DefaultApplyRootMotionPositionX = ExtraData.DefaultApplyRootMotionPositionX;
                rootMotionInfo.DefaultApplyRootMotionPositionY = ExtraData.DefaultApplyRootMotionPositionY;
            }
        }

        protected void CreateBoneFollowers(Transform parent, SkeletonRenderer skeletonRenderer)
        {
            if (ExtraData == null) return;

            var folderInfo = ExtraData.GetFolderInfo(UnitySpineSettings.Instance.EffectBoneFollower);
            if (folderInfo == null) return;

            foreach (var info in folderInfo.Attachments)
            {
                // var newObject = new GameObject($"BoneFollower_{name}");
                var newObject = new GameObject(info.Name);// 取得したいときにイベント名と同名の方が都合が良い
                newObject.transform.SetParent(parent, worldPositionStays: false);
                newObject.transform.ResetLocalTransform();
                newObject.SetLayerRecursively(parent.gameObject.layer);

                var follower = newObject.AddComponent<BoneFollower>();
                follower.skeletonRenderer = skeletonRenderer;
                follower.Initialize();
                // Debug.Log($"Add BoneFollower: boneName={info.Name}");
                follower.SetBone(info.Name);

                // MEMO: boneNameに設定だとうまく動かない
                // https://zenn.dev/happy_elements/articles/a9bbe3c99aefc5
                // follower.boneName = name;

                AddFollowObject(info.Name, newObject);
            }
        }

        protected void CreatePointFollowers(Transform parent, SkeletonRenderer skeletonRenderer)
        {
            if (ExtraData == null) return;

            var folderInfo = ExtraData.GetFolderInfo(UnitySpineSettings.Instance.EffectPointFollower);
            if (folderInfo == null) return;

            foreach (var info in folderInfo.Attachments)
            {
                // var newObject = new GameObject($"BoneFollower_{name}");
                var newObject = new GameObject(info.Name);// 取得したいときにイベント名と同名の方が都合が良い
                newObject.transform.SetParent(parent, worldPositionStays: false);
                newObject.transform.ResetLocalTransform();
                newObject.SetLayerRecursively(parent.gameObject.layer);

                var follower = newObject.AddComponent<PointFollower>();
                follower.skeletonRenderer = skeletonRenderer;
                follower.Initialize();
                // Debug.Log($"Add PointFollower: slotName={info.Name}");
                follower.slotName = info.Name;

                AddFollowObject(info.Name, newObject);
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


        protected void CreateFolders(Transform root, SkeletonRenderer skeletonRenderer,
            SpineExtraDataScriptableObject.FolderInfo folderInfo)
        {
            var folderSetting = UnitySpineSettings.Instance.GetFolderSetting(folderInfo.FolderName);
            if (folderSetting == null)
            {
                Debug.Assert(false, $"CreateFolders: Not found folderSetting for folder={folderInfo.FolderName}");
                return;
            }

            // Folderオブジェクトの作成
            Transform parent = ExtraDataUtility.CreateFolderObject(folderSetting.FolderName, root).transform;

            // Spineのアタッチメントボーン直下のノード名リストを回す
            foreach (var info in folderInfo.Attachments)
            {
                // フォロワーオブジェクトの作成
                GameObject followerObject = ExtraDataUtility.CreateFollowerObject(info, parent, skeletonRenderer, folderSetting);
                if (followerObject == null) throw new System.Exception("followObject is null");

                // レイヤーの設定
                ExtraDataUtility.ApplyLayerSetting(followerObject, folderSetting);

                AddFollowObject(info.Name, followerObject);

                // コールバック
                InvokeCreatedAttachment(followerObject, folderInfo);
            }
        }

        #endregion
    }
}
#nullable restore