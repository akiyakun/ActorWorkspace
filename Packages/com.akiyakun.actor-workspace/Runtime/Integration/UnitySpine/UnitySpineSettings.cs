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
        #region Folders
        [Header("Folders")]
        [SerializeField] string collisionBoxFolder = "3_CollisionBoxFolder";
        public string CollisionBoxFolder => collisionBoxFolder;

        [SerializeField] string hurtBoxFolder = "4_HurtBoxFolder";
        public string HurtBoxFolder => hurtBoxFolder;

        [SerializeField] string hitBoxFolder = "5_HitBoxFolder";
        public string HitBoxFolder => hitBoxFolder;


        public enum FollowerType
        {
            [InspectorName("BoundingBoxFollower (Spine標準)")]
            BoundingBoxFollower,

            [InspectorName("BoundingBox2DFollower (AWカスタム実装)")]
            BoundingBox2DFollower,

            [InspectorName("BoundingBox3DFollower (AWカスタム実装)")]
            BoundingBox3DFollower,
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
        }

        [Serializable]
        public class FolderSetting
        {
            public string FolderName = "";
            public FollowerType FollowerType = FollowerType.BoundingBox2DFollower;

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
        [SerializeField] string effectBoneFollower = "EffectBoneFollower";
        public string EffectBoneFollower => effectBoneFollower;

        [SerializeField] string effectPointFollower = "EffectPointFollower";
        public string EffectPointFollower => effectPointFollower;
        #endregion


#if UNITY_EDITOR
        /// <summary>
        /// フォルダーのリストを取得
        /// </summary>
        List<string> GetFolderList()
        {
            return new List<string>()
            {
                CollisionBoxFolder,
                HurtBoxFolder,
                HitBoxFolder,
            };
        }

        void Reset()
        {
            folderSettings = new();
            foreach (var folder in GetFolderList())
            {
                folderSettings.Add(new FolderSetting()
                {
                    FolderName = folder,
                    FollowerType = FollowerType.BoundingBoxFollower,
                    LayerSetting = LayerSetting.SameAsParent,
                    LayerPairList = new(),
                });
            }
        }
#endif
    }
}
#nullable restore