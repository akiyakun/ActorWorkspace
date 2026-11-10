#nullable enable
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using afl;

namespace ActorWorkspace.UnitySpine
{
    // SkeletonDataAssetの追加情報
    // [CreateAssetMenu(fileName = "SkeletonDataMapping", menuName = "Spine/Skeleton Data Mapping")]
    // FIXME: 当たり判定の追加情報(IAWHitAreaInfoProvider。hurtBoxes・attackBindings)は使っていないのでコメントアウトした(2026-11-10)。攻撃ごとのデータ(AttackData)の形が決まったら戻す
    public class SpineExtraDataScriptableObject : ScriptableObjectCustom, IAWExtraData //, IAWHitAreaInfoProvider
    {
        public bool DefaultApplyRootMotionPositionX = true;
        public bool DefaultApplyRootMotionPositionY;


        // アタッチメントを取得しやすいようにするための入れ物
        [System.Serializable]
        public class FolderInfo : IAWAttachmentInfo
        {
            // From IAWAttachmentInfo
            public string AttachmentFolderName => folderName;

            [SerializeField] string folderName = "";
            public string FolderName => folderName;

            [SerializeField] List<SpineNodeInfo> attachments = new();
            public IReadOnlyList<SpineNodeInfo> Attachments => attachments;

            public FolderInfo(string folderName)
            {
                this.folderName = folderName;
            }

            public void AddAttachment(SpineNodeType nodeType, string name, bool alertAlreadyExist = true)
            {
                if (attachments.Contains(name, (t, n) => t.Name == n) == false)
                {
                    attachments.Add(new SpineNodeInfo() { NodeType = nodeType, Name = name });
                }
                else if (alertAlreadyExist == true)
                {
                    Debug.LogError($"AttachmentNames already contains bone name={name}");
                }
            }

            public SpineNodeInfo? GetAttachment(string name)
            {
                foreach (var info in attachments)
                {
                    if (info.Name == name) return info;
                }
                return null;
            }
        }

        // public Dictionary<string, AttachmentInfo> AttachmentTable { get; private set; } = new();
        [SerializeField]
        public List<FolderInfo> folders = new();
        public IReadOnlyList<FolderInfo> Folders => folders;

        [SerializeField, Disable]
        public bool IsImportError = true;

        // 当たり判定の追加情報(IAWHitAreaInfoProvider)。Spine フォルダの extra_data.json の hurtBoxes・attackBindings から取り込む
        // FIXME: 使っていないのでコメントアウトした。AttackData の形が決まったら戻す(シリアライズする欄なので、戻すとアセットに差分が出る)
        // [SerializeField] List<AWHurtBoxInfo> hurtBoxes = new();
        // [SerializeField] List<AWAttackBinding> attackBindings = new();
        // public IReadOnlyList<AWHurtBoxInfo> HurtBoxes => hurtBoxes;
        // public IReadOnlyList<AWAttackBinding> AttackBindings => attackBindings;

        public void Clear()
        {
            DefaultApplyRootMotionPositionX = true;
            DefaultApplyRootMotionPositionY = false;

            folders?.Clear();
            folders = new();

            // hurtBoxes = new();
            // attackBindings = new();
        }

        // FIXME: 使っていないのでコメントアウトした。AttackData の形が決まったら戻す
        // // 同じ名前・同じ番号があればエラーを出して足さない
        // public void AddHurtBoxInfo(AWHurtBoxInfo info)
        // {
        //     if (TryGetHurtBoxInfo(info.Name, out _))
        //     {
        //         Debug.LogError($"SpineExtraData: HurtBox の名前が重なっています。name={info.Name}");
        //         return;
        //     }
        //     hurtBoxes.Add(info);
        // }
        //
        // public void AddAttackBinding(AWAttackBinding binding)
        // {
        //     if (TryGetAttackBinding(binding.LocalId, out _))
        //     {
        //         Debug.LogError($"SpineExtraData: 攻撃の番号が重なっています。localId={binding.LocalId}");
        //         return;
        //     }
        //     attackBindings.Add(binding);
        // }
        //
        // // From IAWHitAreaInfoProvider
        // public bool TryGetHurtBoxInfo(string hurtBoxName, out AWHurtBoxInfo info)
        // {
        //     foreach (var item in hurtBoxes)
        //     {
        //         if (item.Name == hurtBoxName)
        //         {
        //             info = item;
        //             return true;
        //         }
        //     }
        //     info = default;
        //     return false;
        // }
        //
        // // From IAWHitAreaInfoProvider
        // public bool TryGetAttackBinding(int localId, out AWAttackBinding binding)
        // {
        //     foreach (var item in attackBindings)
        //     {
        //         if (item.LocalId == localId)
        //         {
        //             binding = item;
        //             return true;
        //         }
        //     }
        //     binding = default;
        //     return false;
        // }

        public FolderInfo? GetFolderInfo(string key, bool createIfNotExist = false)
        {
            foreach (var info in Folders)
            {
                if (info.FolderName == key) return info;
            }

            if (createIfNotExist)
            {
                var info = new FolderInfo(key);
                // Attachments.Add(key, info);
                folders.Add(info);
                return info;
            }

            return null;
        }

        // public IReadOnlyList<string>? GetAttachmentNames(string key)
        // {
        //     var info = GetFolderInfo(key);
        //     if (info == null) return null;
        //     return info.AttachmentNames;
        // }

        public void AddAttachment(string folderName, SpineNodeType nodeType, string name, bool alertAlreadyExist = true)
        {
            var info = GetFolderInfo(folderName, createIfNotExist: true)
                ?? throw new System.NullReferenceException();
            info.AddAttachment(nodeType, name, alertAlreadyExist);
        }

        public SpineNodeInfo? GetAttachment(string folderName, string name)
        {
            var info = GetFolderInfo(folderName);
            if (info == null) return null;
            return info.GetAttachment(name);
        }

    }
}
#nullable restore