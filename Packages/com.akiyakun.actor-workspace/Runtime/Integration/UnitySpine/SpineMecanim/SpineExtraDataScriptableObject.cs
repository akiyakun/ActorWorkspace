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
        // FIXME: とりあえずここに書いてる
        public const string EffectBoneFollower = "EffectBoneFollower";
        public const string EffectPointFollower = "EffectPointFollower";

        public const string CollisionBoxFollower = "3_CollisionBoxFolder";
        public const string HurtBoxFollower = "4_HurtBoxFolder";
        public const string HitBoxFollower = "5_HitBoxFolder";

        // アタッチメントを取得しやすいようにするための入れ物
        [System.Serializable]
        public class AttachmentInfo
        {
            public string key = "";
            public string Key => key;

            [SerializeField]
            List<string> attachmentNames = new();
            public IReadOnlyList<string> AttachmentNames => attachmentNames;

            public void AddAttachmentName(string name, bool alertAlreadyExist = true)
            {
                if (attachmentNames.Contains(name) == false)
                {
                    attachmentNames.Add(name);
                }
                else if (alertAlreadyExist == true)
                {
                    Debug.LogError($"AttachmentNames already contains bone name={name}");
                }
            }
        }
        // public Dictionary<string, AttachmentInfo> AttachmentTable { get; private set; } = new();
        [SerializeField]
        public List<AttachmentInfo> attachments = new();
        public IReadOnlyList<AttachmentInfo> Attachments => attachments;

        public void Clear()
        {
            attachments = new();
        }

        public AttachmentInfo? GetAttachmentInfo(string key, bool createIfNotExist = false)
        {
            // if (Attachments.TryGetValue(key, out var info))
            // {
            //     return info;
            // }
            foreach (var info in Attachments)
            {
                if (info.Key == key) return info;
            }

            if (createIfNotExist)
            {
                var info = new AttachmentInfo() { key = key };
                // Attachments.Add(key, info);
                attachments.Add(info);
                return info;
            }

            return null;
        }

        public IReadOnlyList<string>? GetAttachmentNames(string key)
        {
            var info = GetAttachmentInfo(key);
            if (info == null) return null;
            return info.AttachmentNames;
        }

        public void AddAttachmentName(string key, string name, bool alertAlreadyExist = true)
        {
            var info = GetAttachmentInfo(key, createIfNotExist: true)
                ?? throw new System.NullReferenceException();
            info.AddAttachmentName(name, alertAlreadyExist);
        }

    }
}
#nullable restore