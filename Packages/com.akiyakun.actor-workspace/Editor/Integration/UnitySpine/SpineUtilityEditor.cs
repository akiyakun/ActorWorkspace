using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
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


    }
}
