using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.Editor.UnitySpine
{
    public static class SpineUtilityEditor
    {
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

        public static SkeletonAnimation CreateSkeletonAnimationFromAssetDatabae(string path)
        {
            var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
            Debug.Assert(skeletonDataAsset != null, $"SkeletonDataAsset not found at path: {path}");
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
            var spineShaderGUIType = Type.GetType("SpineSpriteShaderGUI, spine-unity-editor");
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
            var blendModeValue = Enum.ToObject(blendModeEnum, blendMode);

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
            Debug.Assert(skeletonData != null);

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
            Debug.Assert(skeletonData != null);

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
