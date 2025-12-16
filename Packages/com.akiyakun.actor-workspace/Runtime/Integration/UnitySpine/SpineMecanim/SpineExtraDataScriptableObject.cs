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
    public class SpineExtraDataScriptableObject : ScriptableObjectCustom
    {
        [SerializeField] public List<string> FollowBoneList = null!;
        [SerializeField] public List<string> FollowPointList = null!;

        public void Clear()
        {
            FollowBoneList?.Clear();
            FollowPointList?.Clear();
        }

        public void AddFollowBoneName(string name)
        {
            FollowBoneList ??= new();

            if (FollowBoneList.Contains(name) == false)
            {
                FollowBoneList.Add(name);
            }
            else
            {
                Debug.LogWarning($"[SpineExtraData] FollowBoneList already contains bone name: {name}");
            }
        }

        public void AddFollowPointName(string name)
        {
            FollowPointList ??= new();

            if (FollowPointList.Contains(name) == false)
            {
                FollowPointList.Add(name);
            }
            else
            {
                Debug.LogWarning($"[SpineExtraData] FollowPointList already contains point name: {name}");
            }
        }
    }
}
#nullable restore