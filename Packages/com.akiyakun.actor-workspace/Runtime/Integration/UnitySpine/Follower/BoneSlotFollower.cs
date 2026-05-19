#nullable enable
using UnityEngine;
using Spine.Unity;
using System;
using afl;

namespace ActorWorkspace.UnitySpine
{
    // FIXME: クラス名
    // MEMO: Spine.Unity.BoneFollower を参考にして作成
    public class BoneSlotFollower : MonoBehaviour, IAWFollower
    {
        // default値はBoneFollowerに合わせてある
        #region BoneFollower Like Fields
        public SkeletonRenderer skeletonRenderer = null!;

        [SpineBone(dataField: "skeletonRenderer")]
        // public string boneName;
        public string boneName
        {
            get => _boneName;
            set
            {
                if (_boneName != value)
                {
                    _boneName = value;
                    BoneNameHash = Utility.StringToHashId(_boneName);
                }
            }
        }

        public bool followXYPosition = true;

        // BoneFollowerとは違う仕組みが欲しくなるかも？接触させないために奥に移動させたい等
        // public bool followZPosition = true;

        public bool followLocalScale = false;
        public bool followParentWorldScale = false;
        #endregion

        public event System.Action<IAWFollower, bool>? OnActiveChange;

        string _boneName = string.Empty;
        public int BoneNameHash { get; private set; }
        [NonSerialized, Disable] public string? FolderName;

        bool isInitialized = false;
        Spine.Bone bone = null!;
        Spine.Slot? slot;


        protected virtual void Awake()
        {
            if (skeletonRenderer != null)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (isInitialized) return;

            if (skeletonRenderer == null)
            {
                throw new System.Exception("skeletonRenderer is null");
            }

            if (skeletonRenderer.gameObject.TryGetComponent<ISkeletonAnimation>(out var skeletonAnimation))
            {
                // MEMO: UpdateComplete を使うか LateUpdate() を使うか迷いどころ
                // GameObject.SetActive()使おうとすると、LateUpdate() では対応できない。
                // UpdateComplete ならGameObjectの状態と関係なくコールバックされるので非アクティブでも呼ばれる。
                skeletonAnimation.UpdateComplete += OnSkeletonUpdated;
            }
            else
            {
                throw new System.Exception("SkeletonAnimation component not found");
            }

            bone = skeletonRenderer.Skeleton.FindBone(boneName);
            if (bone == null) throw new System.Exception($"Bone not found: {boneName}");

            slot = skeletonRenderer.Skeleton.FindSlot(boneName);
            // if (slot == null) throw new System.Exception($"Slot not found: {boneName}");

            isInitialized = true;
        }

        void OnDestroy()
        {
            if (skeletonRenderer != null)
            {
                if (skeletonRenderer.gameObject.TryGetComponent<ISkeletonAnimation>(out var skeletonAnimation))
                {
                    skeletonAnimation.UpdateComplete -= OnSkeletonUpdated;
                }
            }
        }

        void OnSkeletonUpdated(ISkeletonAnimation sa)
        {
            if (isInitialized == false)
            {
                Debug.Assert(false);
                return;
            }

            Vector3 localPosition = transform.localPosition;
            if (followXYPosition == true)
            {
                localPosition.x = bone.WorldX;
                localPosition.y = bone.WorldY;
                // localPosition.z = localPosition.z;
            }
            transform.localPosition = localPosition;

            Vector3 localScale = transform.localScale;
            if (followParentWorldScale && bone.Parent != null)
            {
                localScale = new Vector3(bone.WorldScaleX, bone.WorldScaleY, localScale.z);
            }
            if (followLocalScale)
            {
                localScale.Scale(new Vector3(bone.ScaleX, bone.ScaleY, 1.0f));
            }
            transform.localScale = localScale;

            if (slot != null)
            {
                Spine.Attachment currentAttachment = slot.Attachment;
                if (currentAttachment == null)
                {
                    // スロットにアタッチメントが設定されていない場合は非表示にする
                    if (gameObject.activeSelf == true)
                    {
                        OnActiveChange?.Invoke(this, false);
                        gameObject.SetActive(false);
                    }
                }
                else
                {
                    // スロットにアタッチメントが設定されている場合は表示する
                    if (gameObject.activeSelf == false)
                    {
                        // if (gameObject.activeInHierarchy == true)
                        OnActiveChange?.Invoke(this, true);
                        gameObject.SetActive(true);
                    }
                }
                // gameObject.SetActive(slot.Bone.Active);
            }

        }
    }
}
#nullable restore