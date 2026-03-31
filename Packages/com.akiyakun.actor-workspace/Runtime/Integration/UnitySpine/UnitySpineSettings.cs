#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;
using System;

namespace ActorWorkspace.UnitySpine
{
    // パッケージが使用する設定データ
    // MEMO: SpineSettings だと本家と名前が重複する
    // See also: SpineSettingsScriptableObjectEditor.cs
    [CreateAssetMenu(menuName = Environment.AssetMenuRoot + "UnitySpineSettings (ScriptableObject)", fileName = "UnitySpineSettings")]
    public class UnitySpineSettings : SingletonScriptableObject<UnitySpineSettings>
    {
        #region Import Settings
        [Header("Import Settings")]
        [SerializeField] List<string> importTargetDirectories = new() { "Assets" };
        public IReadOnlyList<string> ImportTargetDirectories => importTargetDirectories;

        [SerializeField] UnitySpineImportCallback? importCallback = null;
        public UnitySpineImportCallback? ImportCallback => importCallback;
        #endregion


        #region Folders
        [Header("Folders")]

        [SerializeField] string anchorFolder = "1_AnchorFolder";
        public string AnchorFolder => anchorFolder;

        [SerializeField] string collisionBoxFolder = "3_CollisionBoxFolder";
        public string CollisionBoxFolder => collisionBoxFolder;

        [SerializeField] string hurtBoxFolder = "4_HurtBoxFolder";
        public string HurtBoxFolder => hurtBoxFolder;

        [SerializeField] string hitBoxFolder = "5_HitBoxFolder";
        public string HitBoxFolder => hitBoxFolder;


        public enum FollowerType
        {
            [InspectorName("BoneFollower (Spine標準)")]
            BoneFollower,

            [InspectorName("PointFollower (Spine標準)")]
            PointFollower,

            [InspectorName("BoundingBoxFollower (Spine標準)")]
            BoundingBoxFollower,

            [InspectorName("BoundingBox2DFollower (AWカスタム実装)")]
            BoundingBox2DFollower,

            [InspectorName("BoundingBox2DFollowerIsTrigger (AWカスタム実装)")]
            BoundingBox2DFollowerIsTrigger,

            [InspectorName("BoundingBox3DFollower (AWカスタム実装)")]
            BoundingBox3DFollower,

            [InspectorName("BoundingBox3DFollowerIsTrigger (AWカスタム実装)")]
            BoundingBox3DFollowerIsTrigger,
        }

        public enum LayerSetting
        {
            // 親と同じレイヤーに設定
            SameAsParent,

            // レイヤー毎に設定
            PerLayer,
        }

        [Serializable]
        public class LayerPair
        {
            [SerializeField, LayerSelector]
            public int SourceLayer;

            [SerializeField, LayerSelector]
            public int TargetLayer;

            [SerializeField]
            public LayerMask IncludeLayerMask;

            [SerializeField]
            public LayerMask ExcludeLayerMask;

            [SerializeField]
            public LayerMask ContactCaptureLayers;

            [SerializeField]
            public LayerMask CallbackLayers;
        }

        [Serializable]
        public class FolderSetting
        {
            public string FolderName = "";
            public FollowerType FollowerType = FollowerType.BoundingBox2DFollower;

            public string Options = string.Empty;

            public LayerSetting LayerSetting;
            public List<LayerPair> LayerPairList = new();
        }
        [SerializeField] List<FolderSetting> folderSettings = new();
        public IReadOnlyList<FolderSetting> FolderSettings => folderSettings;

        public FolderSetting? GetFolderSetting(string folderName)
        {
            foreach (var setting in FolderSettings)
            {
                if (setting.FolderName == folderName)
                {
                    return setting;
                }
            }
            return null;
        }

        #endregion


        #region Effects
        [Header("Effects")]
        [SerializeField] string effectBoneFollower = "8_EffectBoneFolder";
        public string EffectBoneFollower => effectBoneFollower;

        [SerializeField] string effectPointFollower = "9_EffectPointFolder";
        public string EffectPointFollower => effectPointFollower;
        #endregion


#if UNITY_EDITOR
        public bool CheckTargetDirectory(string path)
        {
            foreach (var dir in importTargetDirectories)
            {
                if (string.IsNullOrEmpty(dir)) continue;
                if (path.StartsWith(dir))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// フォルダーのリストを取得
        /// </summary>
        List<(string, FollowerType)> GetFolderList()
        {
            return new List<(string, FollowerType)>()
            {
                (CollisionBoxFolder, FollowerType.BoundingBox2DFollower),
                (HurtBoxFolder, FollowerType.BoundingBox2DFollower),
                (HitBoxFolder, FollowerType.BoundingBox2DFollower),
                (EffectBoneFollower, FollowerType.BoneFollower),
                (EffectPointFollower, FollowerType.PointFollower),
            };
        }

        void Reset()
        {
            folderSettings = new();
            foreach (var folder in GetFolderList())
            {
                folderSettings.Add(new FolderSetting()
                {
                    FolderName = folder.Item1,
                    FollowerType = folder.Item2,
                    LayerSetting = LayerSetting.SameAsParent,
                    LayerPairList = new(),
                });
            }
        }
#endif
    }
}
#nullable restore