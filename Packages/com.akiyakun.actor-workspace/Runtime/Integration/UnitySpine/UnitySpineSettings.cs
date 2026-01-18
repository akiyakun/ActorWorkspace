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
            BoundingBox2DFollower,
            BoundingBox3DFollower,
        }

        public enum LayerSetting
        {
            // 親と同じレイヤーに設定
            Parent,

            // レイヤー毎に設定
            LayerPair,
        }

        [Serializable]
        public class LayerPair
        {
            [SerializeField, LayerSelector]
            public int InLayer;

            [SerializeField, LayerSelector]
            public int ToLayer;
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


        /// <summary>
        /// フォルダーのリストを取得
        /// </summary>
        public List<string> GetFolderList()
        {
            return new List<string>()
            {
                CollisionBoxFolder,
                HurtBoxFolder,
                HitBoxFolder,
            };
        }
        #endregion


        #region Effects
        [Header("Effects")]
        [SerializeField] string effectBoneFollower = "EffectBoneFollower";
        public string EffectBoneFollower => effectBoneFollower;

        [SerializeField] string effectPointFollower = "EffectPointFollower";
        public string EffectPointFollower => effectPointFollower;
        #endregion

    }
}
#nullable restore