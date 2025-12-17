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
        [SerializeField] List<string> followBoneNameList = new();
        public IReadOnlyList<string> FollowBoneNameList => followBoneNameList;

        [SerializeField] public List<string> followPointNameList = new();
        public IReadOnlyList<string> FollowPointNameList => followPointNameList;

        public void Clear()
        {
            followBoneNameList?.Clear();
            followPointNameList?.Clear();
        }

        public void AddFollowBoneName(string name)
        {
            if (followBoneNameList.Contains(name) == false)
            {
                followBoneNameList.Add(name);
            }
            else
            {
                Debug.LogWarning($"[SpineExtraData] FollowBoneList already contains bone name: {name}");
            }
        }

        public void AddFollowPointName(string name)
        {
            if (followPointNameList.Contains(name) == false)
            {
                followPointNameList.Add(name);
            }
            else
            {
                Debug.LogWarning($"[SpineExtraData] FollowPointList already contains point name: {name}");
            }
        }
    }
}
#nullable restore