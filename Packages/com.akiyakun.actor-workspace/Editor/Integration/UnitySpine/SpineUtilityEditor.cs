#nullable enable
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using afl;
using afl.Editor;

namespace ActorWorkspace.Editor.UnitySpine
{
    public static class SpineUtilityEditor
    {
        //extra_data.json
        public const string ExtraDataJsonFileName = "extra_data.json";
        public const string SpineExtraDataSuffix = "_SpineExtraData";

        // Copy from SpineSpriteShaderGUI
        public enum eBlendMode
        {
            // "PMA Vertex, PMA Texture"
            PreMultipliedAlpha,
            // "PMA Vertex, Straight Texture"
            PreMultipliedVertexAlpha,

            StandardAlpha,
            Opaque,
            Additive,
            SoftAdditive,
            Multiply,
            Multiplyx2,
        };


        #region SkeletonData
        // SpineのjsonパスからSkeletonDataAssetのパスを取得する
        public static string GetSkeletonDataPath(string spineJsonPath)
        {
            return $"{Utility.GetPathWithoutExtension(spineJsonPath)}{AssetUtility.SkeletonDataSuffix}.asset";
        }
        #endregion


        #region SpineExtraData
        public static string GetSpineExtraDataPath(string skeletonDataAssetPath)
        {
            string name = Path.GetFileNameWithoutExtension(skeletonDataAssetPath).Replace(AssetUtility.SkeletonDataSuffix, "");
            return $"{Path.GetDirectoryName(skeletonDataAssetPath)}/{name}{SpineExtraDataSuffix}.asset";
        }

        public static bool IsSpineExtraDataExists(string skeletonDataAssetPath)
        {
            return File.Exists(GetSpineExtraDataPath(skeletonDataAssetPath));
        }
        #endregion


        #region Mecanim
        // AnimatorControllerファイルのAnimationClipの生成(再生成)
        // See also: https://ja.esotericsoftware.com/forum/d/14630-c-force-update-animationclips
        public static void GenerateMecanimAnimationClip(string path)
        {
            var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
            if (skeletonDataAsset == null || skeletonDataAsset.controller == null) return;

            D.LogVerbose($"[SpineMecanimPostprocessor] Updating Mecanim AnimationClips: {skeletonDataAsset.name}");

            // MEMO: GenerateMecanimAnimationClip()内でSaveAssets()している
            SkeletonBaker.GenerateMecanimAnimationClips(skeletonDataAsset);
        }
        #endregion


        public static AnimatorEditorUtility.AnimatorControllerInfo? PreSetupAnimatorController(SkeletonDataAsset skeletonDataAsset)
        {
            if (skeletonDataAsset == null)
            {
                Debug.LogWarning("SkeletonDataAsset is null.");
                return null;
            }

            if (skeletonDataAsset.controller == null)
            {
                Debug.LogWarning("SkeletonDataAsset has no AnimatorController.");
                return null;
            }

            var editorAnimatorController = skeletonDataAsset.controller as UnityEditor.Animations.AnimatorController;
            if (editorAnimatorController == null)
            {
                Debug.LogWarning("SkeletonDataAsset's controller is not an AnimatorController.");
                return null;
            }

            AnimatorEditorUtility.AnimatorControllerInfo animatorControllerInfo = AnimatorEditorUtility.GetAnimatorControllerInfo(editorAnimatorController);
            SetLoopForLoopSuffix(animatorControllerInfo);

            // AssetDatabase.SaveAssets();

            return animatorControllerInfo;
        }


        // "_loop"サフィックスのAnimationClipをループ設定にする
        public static void SetLoopForLoopSuffix(AnimatorEditorUtility.AnimatorControllerInfo animatorControllerInfo)
        {
            foreach (var clip in animatorControllerInfo.AnimationClips)
            {
                if (clip.name.EndsWith("_loop"))
                {
                    // 既存の AnimationClipSettings を取得
                    var settings = AnimationUtility.GetAnimationClipSettings(clip);

                    // ループを有効にする
                    if (!settings.loopTime)
                    {
                        settings.loopTime = true;
                        AnimationUtility.SetAnimationClipSettings(clip, settings);
                        EditorUtility.SetDirty(clip);
                        // Debug.Log($"[SpineLoopClipSetter] Set loopTime = true for {clip.name}");
                    }
                }
            }

            // AssetDatabase.SaveAssets();
        }

        // AssetDatabaseからSkeletonDataAssetを読み込みGameObjectを作成する
        // 主にビューワーやデバッグ用途
        public static SkeletonAnimation CreateSkeletonAnimationFromAssetDatabase(string path)
        {
            var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
            if (skeletonDataAsset == null) throw new System.Exception($"SkeletonDataAsset not found at path: {path}");
            var newSkeleton = new GameObject(skeletonDataAsset.name);
            var skeletonAnimation = newSkeleton.AddComponent<SkeletonAnimation>();
            skeletonAnimation.skeletonDataAsset = skeletonDataAsset;
            skeletonAnimation.Initialize(true);

            // skeletonAnimation.skeleton.SetSkin(skins.Items[skinIndex]);
            skeletonAnimation.Skeleton.SetSlotsToSetupPose();

            return skeletonAnimation;
        }

        // Spineのマテリアルのブレンドモードを設定する
        // See also: spine-unity/Editor/Shaders/SpineSpriteShaderGUI.cs
        public static void SetBlendMode(Material material, eBlendMode blendMode)
        {
            // SpineSpriteShaderGUIの型を取得
            var spineShaderGUIType = System.Type.GetType("SpineSpriteShaderGUI, spine-unity-editor");
            if (spineShaderGUIType == null)
            {
                Debug.LogError("SpineSpriteShaderGUI型が見つかりません");
                return;
            }

            // eBlendModeのenum型を取得
            var blendModeEnum = spineShaderGUIType.GetNestedType("eBlendMode", BindingFlags.NonPublic);
            if (blendModeEnum == null)
            {
                Debug.LogError("eBlendMode型が見つかりません");
                return;
            }

            // enum値を生成
            var blendModeValue = System.Enum.ToObject(blendModeEnum, blendMode);

            // SetBlendModeメソッドを取得
            var setBlendModeMethod = spineShaderGUIType.GetMethod("SetBlendMode", BindingFlags.Static | BindingFlags.NonPublic);
            if (setBlendModeMethod == null)
            {
                Debug.LogError("SetBlendModeメソッドが見つかりません");
                return;
            }

            // 呼び出し
            setBlendModeMethod.Invoke(null, new object[] { material, blendModeValue });
        }

        public static void SetAtlasTextureFilterMode(SkeletonDataAsset skeletonDataAsset, FilterMode filterMode)
        {
            if (skeletonDataAsset == null || skeletonDataAsset.atlasAssets == null) return;

            foreach (AtlasAssetBase atlasAsset in skeletonDataAsset.atlasAssets)
            {
                if (atlasAsset == null) continue;

                foreach (var material in atlasAsset.Materials)
                {
                    string assetPath = AssetDatabase.GetAssetPath(material.mainTexture);
                    var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (importer != null)
                    {
                        importer.filterMode = filterMode;
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        // 特定の名前のボーン以下に存在するボーンを取得する方法
        public static List<BoneData> GetBonesUnderBone(SkeletonData skeletonData, string boneName, int depth = 0)
        {
            if (skeletonData == null) throw new System.ArgumentNullException(nameof(skeletonData));

            var result = new List<BoneData>();

            // ボーン名から BoneData を取得
            BoneData targetBone = skeletonData.FindBone(boneName);
            if (targetBone == null)
            {
                // Debug.LogError($"Bone not found: boneName={boneName}, skeletonData={skeletonData.Name}");
                return result;
            }

            // スケルトンデータのすべてのボーンをチェック
            foreach (BoneData boneData in skeletonData.Bones.Items)
            {
                if (targetBone.Name == boneData.Name) continue;

                // ボーンが指定されたボーンの子孫かどうかを判定
                if (IsDescendantOf(boneData.Parent, targetBone, depth))
                {
                    // Debug.Log(boneData.Name);
                    result.Add(boneData);
                }
            }

            return result;
        }

        // 特定の名前のボーン以下に存在するスロットを取得する方法
        public static List<SlotData> GetSlotsUnderBone(SkeletonData skeletonData, string boneName, int depth = 0)
        {
            if (skeletonData == null) throw new System.ArgumentNullException(nameof(skeletonData));

            var result = new List<SlotData>();

            // ボーン名から BoneData を取得
            BoneData targetBone = skeletonData.FindBone(boneName);
            if (targetBone == null)
            {
                // Debug.LogError($"Bone not found: boneName={boneName}, skeletonData={skeletonData.Name}");
                return result;
            }

            // スケルトンデータのすべてのスロットをチェック
            foreach (SlotData slotData in skeletonData.Slots.Items)
            {
                if (slotData.BoneData == null) continue;

                // if (slotData.Name == "dummy") continue;

                // スロットが属するボーンを取得
                // MEMO: BoneDataを取得している時点で1階層上を見ている
                if (IsDescendantOf(slotData.BoneData, targetBone, depth))
                {
                    // Debug.Log(slotData.Name);
                    result.Add(slotData);
                }
            }

            return result;
        }

        /// <summary>
        /// ボーンが指定されたボーンの子孫かどうかを判定
        /// </summary>
        /// <param name="parent">確認したいボーン(親ボーンを渡す)</param>
        /// <param name="ancestor">親ボーン</param>
        /// <param name="depth">深さの制限。0以下の場合は制限なし。</param>
        public static bool IsDescendantOf(BoneData parent, BoneData ancestor, int depth = 0)
        {
            while (parent != null)
            {
                if (parent == ancestor) return true;
                parent = parent.Parent;

                depth--;
                if (depth == 0) return false;
            }
            return false;
        }


    }
}
#nullable restore