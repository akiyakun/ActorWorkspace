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


        #region Export Settings
        // 一括書き出し(SpineBulkExporter)で使う
        [Header("Export Settings")]

        // Spine の元データのフォルダ(Unity プロジェクトのフォルダからの相対パス)
        // この下の <カテゴリ>/<名前>@bundle のフォルダを書き出す
        [SerializeField] string exportSourceDirectory = "../spine";
        public string ExportSourceDirectory => exportSourceDirectory;

        // 書き出し先のフォルダ(Unity プロジェクトのフォルダからの相対パス)。この下の <カテゴリ>/<名前>@bundle に書き出す
        [SerializeField] string exportDestinationDirectory = "Assets/AssetBundleData/Actor";
        public string ExportDestinationDirectory => exportDestinationDirectory;
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

        // 判定の箱(BoundingBox 系のフォロワー)のスロットの絵を出すか。ふだんは出さない
        // 実行中は SpineFolderSlotVisibility.IsVisible で切り替える(デバッグで当たり判定を見るときなど)
        [SerializeField] bool showFolderSlots = false;
        public bool ShowFolderSlots => showFolderSlots;


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


        #region Collision Box Visualizer
        // ゲーム中に当たり判定(HurtBox・HitBox)を描くときの色(描くのはゲーム側。ここは色の設定だけ)
        // HurtBox・HitBox は、コライダーの GameObject のレイヤーで色を決める(リストの上から、Layers に入っている最初のもの。無ければ既定の色)
        [Serializable]
        public class CollisionBoxLayerColor
        {
            [Tooltip("この色にするレイヤー(コライダーの GameObject のレイヤー)")]
            public LayerMask Layers;
            public Color Color = Color.white;
        }

        [Header("Collision Box Visualizer")]
        [Tooltip("HurtBox(やられ判定)の色。レイヤーごと")]
        [SerializeField] List<CollisionBoxLayerColor> hurtBoxColors = new();
        [Tooltip("HurtBox の色(どのレイヤーの設定にも入らないとき)")]
        [SerializeField] Color hurtBoxDefaultColor = new(0.3f, 1.0f, 0.3f, 1.0f);

        [Tooltip("HitBox(攻撃判定)の色。レイヤーごと")]
        [SerializeField] List<CollisionBoxLayerColor> hitBoxColors = new();
        [Tooltip("HitBox の色(どのレイヤーの設定にも入らないとき)")]
        [SerializeField] Color hitBoxDefaultColor = new(0.3f, 1.0f, 0.3f, 1.0f);

        [Tooltip("エフェクト(パーティクルの弾など)の HitBox の色")]
        [SerializeField] Color effectHitBoxColor = new(1.0f, 0.2f, 1.0f, 1.0f);

        [Tooltip("HitBox の塗りの不透明度(色のアルファに掛ける)。HurtBox は塗らずに枠線だけ描く")]
        [SerializeField, Range(0.0f, 1.0f)] float hitBoxFillAlpha = 0.3f;

        [Tooltip("枠線の太さ(画面のピクセル)")]
        [SerializeField, Min(1.0f)] float collisionBoxLineWidth = 2.0f;

        public IReadOnlyList<CollisionBoxLayerColor> HurtBoxColors => hurtBoxColors;
        public Color HurtBoxDefaultColor => hurtBoxDefaultColor;
        public IReadOnlyList<CollisionBoxLayerColor> HitBoxColors => hitBoxColors;
        public Color HitBoxDefaultColor => hitBoxDefaultColor;
        public Color EffectHitBoxColor => effectHitBoxColor;
        public float HitBoxFillAlpha => hitBoxFillAlpha;
        public float CollisionBoxLineWidth => collisionBoxLineWidth;

        // レイヤーの HurtBox・HitBox の色
        public Color GetHurtBoxColor(int layer) => FindLayerColor(hurtBoxColors, layer, hurtBoxDefaultColor);
        public Color GetHitBoxColor(int layer) => FindLayerColor(hitBoxColors, layer, hitBoxDefaultColor);

        static Color FindLayerColor(List<CollisionBoxLayerColor> colors, int layer, Color defaultColor)
        {
            int bit = 1 << layer;
            foreach (var color in colors)
            {
                if ((color.Layers.value & bit) != 0) return color.Color;
            }
            return defaultColor;
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