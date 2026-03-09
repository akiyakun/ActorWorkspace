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
    public class SpineExtraDataScriptableObject : ScriptableObjectCustom, IAWExtraData
    {
        // アタッチメントを取得しやすいようにするための入れ物
        [System.Serializable]
        public class FolderInfo : IAWAttachmentInfo
        {
            // From IAWAttachmentInfo
            public string AttachmentName => folderName;

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

        public void Clear()
        {
            folders?.Clear();
            folders = new();
        }

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