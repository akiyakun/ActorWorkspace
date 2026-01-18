#nullable enable
using System.Runtime.CompilerServices;
using UnityEngine;
using Spine.Unity;
using afl;

namespace ActorWorkspace.UnitySpine
{
    public static class ExtraDataUtility
    {
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static string ParseEventName(string value)
        // {
        //     int index = value.IndexOf('=');
        //     if (index < 0) return value;
        //     return value.Substring(0, index);
        // }


        public static GameObject CreateBoundingBoxFollower(SpineNodeInfo nodeInfo, Transform parent, SkeletonRenderer skeletonRenderer)
        {
            // var newObject = new GameObject($"BoneFollower_{name}");
            var newObject = new GameObject(nodeInfo.Name);// 取得したいときにイベント名と同名の方が都合が良い
            newObject.transform.SetParent(parent, worldPositionStays: false);
            newObject.transform.ResetLocalTransform();
            // newObject.SetLayerRecursively(parent.gameObject.layer);

            {

                var follower = newObject.AddComponent<BoundingBoxFollower>();
                follower.skeletonRenderer = skeletonRenderer;
                // slotName を先に設定してから初期化する
                // Debug.Log($"Add PointFollower: slotName={name}");
                follower.slotName = nodeInfo.Name;
                follower.isTrigger = true;

                follower.Initialize();
            }

            // skeletonRenderer.skeleton.FindSlot(name);

            return newObject;
        }

        public static GameObject CreateBoundingBox2DFollower(SpineNodeInfo nodeInfo, Transform parent, SkeletonRenderer skeletonRenderer)
        {
            Debug.Assert(false);
            var newObject = new GameObject(nodeInfo.Name);
            return newObject;
        }

        // BoundingBox3DFollower
        // BoxCollider
        public static GameObject CreateBoundingBox3DFollower(SpineNodeInfo nodeInfo, Transform parent, SkeletonRenderer skeletonRenderer)
        {
            if (nodeInfo.NodeType != SpineNodeType.Bone)
            {
                throw new System.Exception($"CreateBoundingBox3DFollower: スロットではなくボーンを使用してください. name={nodeInfo.Name}");
            }

            var newObject = new GameObject(nodeInfo.Name);
            newObject.transform.SetParent(parent, worldPositionStays: false);
            newObject.transform.ResetLocalTransform();
            // newObject.SetLayerRecursively(parent.gameObject.layer);

            Vector3 v = new Vector3();
            v.Set(32.0f / 100.0f);

            var collider = newObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = v;

            // var follower = newObject.AddComponent<BoneFollower>();
            var follower = newObject.AddComponent<BoneSlotFollower>();
            follower.skeletonRenderer = skeletonRenderer;

            // // slotName を先に設定してから初期化する
            // // Debug.Log($"Add PointFollower: slotName={name}");
            follower.boneName = nodeInfo.Name;
            follower.followXYPosition = true;
            follower.followLocalScale = false;
            follower.followParentWorldScale = true;
            newObject.transform.localScale = v;

            follower.Initialize();

            return newObject;
        }

        // フォロワーオブジェクトの作成
        public static GameObject CreateFollowerObject(SpineNodeInfo nodeInfo, Transform parent, SkeletonRenderer skeletonRenderer,
            UnitySpineSettings.FolderSetting folderSetting)
        {
            return folderSetting.FollowerType switch
            {
                UnitySpineSettings.FollowerType.BoundingBoxFollower => CreateBoundingBoxFollower(nodeInfo, parent, skeletonRenderer),
                UnitySpineSettings.FollowerType.BoundingBox2DFollower => CreateBoundingBox2DFollower(nodeInfo, parent, skeletonRenderer),
                UnitySpineSettings.FollowerType.BoundingBox3DFollower => CreateBoundingBox3DFollower(nodeInfo, parent, skeletonRenderer),
                _ => throw new System.Exception($"CreateFollowerObject: Unsupported FollowerType={folderSetting.FollowerType}"),
            };
        }

        // followerObject以下のオブジェクトにレイヤーを設定する
        // followerObject: 親が設定済みであること
        public static void ApplyLayerSetting(GameObject followerObject, UnitySpineSettings.FolderSetting folderSetting)
        {
            if (followerObject.transform.parent == null)
            {
                Debug.LogError($"ApplyLayerSetting: followerObject's parent is null");
                return;
            }

            int parentLayer = followerObject.transform.parent.gameObject.layer;

            // レイヤーの設定
            switch (folderSetting.LayerSetting)
            {
                // 親と同じレイヤーに設定
                case UnitySpineSettings.LayerSetting.SameAsParent:
                    followerObject.SetLayerRecursively(parentLayer);
                    break;
                // レイヤー毎に設定
                case UnitySpineSettings.LayerSetting.PerLayer:
                    {
                        foreach (var pair in folderSetting.LayerPairList)
                        {
                            // 対象のレイヤーのとき
                            if (pair.SourceLayer == parentLayer)
                            {
                                // 対応するターゲットレイヤーに設定
                                followerObject.SetLayerRecursively(pair.TargetLayer);
                                break;
                            }
                        }
                    }
                    break;
                default:
                    Debug.Assert(false, $"CreateFolders: Unsupported LayerSetting={folderSetting.LayerSetting}");
                    break;
            }
        }

    }
}
#nullable restore