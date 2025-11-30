using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using Unity.VisualScripting; // SkeletonBaker がある名前空間
using System.Linq;

namespace ActorWorkspace.Editor.UnitySpine
{
    public class SpineAssetPostprocessor : AssetPostprocessor
    {
        // 監視対象のフォルダ（プロジェクト相対パス）
        // static const string targetFolder = "Assets/AssetBundleData/Actor";
        const string targetFolder = "Assets";

        static bool reImportGard = false;

        static List<string> targetList;


        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (reImportGard) return;
            reImportGard = true;

            try
            {
                targetList = new List<string>();

                foreach (string path in importedAssets)
                {
                    // 指定フォルダ以下か？
                    if (path.StartsWith(targetFolder) == false) continue;

                    // 拡張子でフィルタリング
                    if (path.EndsWith(".asset") == false) continue;

                    // アセットのタイプでフィルタリング
                    if (AssetDatabase.GetMainAssetTypeAtPath(path) != typeof(SkeletonDataAsset)) continue;

                    targetList.Add(path);
                }

                // 先にクリップを再生成しておく
                GenerateMecanimAnimationClips(targetList);
            }
            finally
            {
                if (targetList.Count > 0)
                {
                    // 1フレーム後に実行
                    // Debug.Log($"2 cout={pendingAssets.Count}");
                    EditorApplication.delayCall -= ProcessPendingAssets;
                    EditorApplication.delayCall += ProcessPendingAssets;
                }
                else
                {
                    reImportGard = false;
                }
            }

        }

        // MEMO:
        // https://ja.esotericsoftware.com/forum/d/14630-c-force-update-animationclips
        static void GenerateMecanimAnimationClips(List<string> targets)
        {
            foreach (var path in targets)
            {
                var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
                if (skeletonDataAsset == null || skeletonDataAsset.controller == null) continue;

                Debug.Log($"[SpineMecanimPostprocessor] Updating Mecanim AnimationClips: {skeletonDataAsset.name}");

                // MEMO: GenerateMecanimAnimationClips()内でSaveAssetes()している
                SkeletonBaker.GenerateMecanimAnimationClips(skeletonDataAsset);
            }
        }

        static void ProcessPendingAssets()
        {
            Debug.Log($"3 cout={targetList.Count}");

            try
            {
                Process(targetList);

                targetList.Clear();

                // AssetDatabase.Refresh();
               AssetDatabase.SaveAssets();

            }
            finally
            {
                reImportGard = false;
            }
        }

        static void Process(List<string> targets)
        {
            foreach (var path in targets)
            {
                var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
                if (skeletonDataAsset == null || skeletonDataAsset.controller == null) continue;

                SetLoopForLoopSuffix(skeletonDataAsset);

                // SpineイベントのInt/Float/Stringすべてをインポート
                ImportAllSpineEventParameters(skeletonDataAsset);
            }
        }

        // SkeletonDataAsset を指定して呼び出す
        public static void SetLoopForLoopSuffix(SkeletonDataAsset sda)
        {
            if (sda == null)
            {
                Debug.LogWarning("SkeletonDataAsset is null.");
                return;
            }

            if (sda.controller == null)
            {
                Debug.LogWarning("SkeletonDataAsset has no AnimatorController.");
                return;
            }

            // AnimatorControllerからすべてのAnimationClipを取得
            var animatorController = sda.controller as UnityEditor.Animations.AnimatorController;
            if (animatorController == null)
            {
                Debug.LogWarning("SkeletonDataAsset's controller is not an AnimatorController.");
                return;
            }
            // var clips = animatorController.animationClips;

            // AnimatorControllerからすべてのAnimationClipを取得
            // var clips = sda.controller.animationClips;
            // if (clips == null || clips.Length == 0) return;

            // controllerAsset 配下の全ての AnimationClip を取得
            var asset = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(animatorController));
            AnimationClip[] clips = System.Array.FindAll(asset, obj => obj is AnimationClip).Cast<AnimationClip>().ToArray();

            foreach (var clip in clips)
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
                        Debug.Log($"[SpineLoopClipSetter] Set loopTime = true for {clip.name}");
                    }
                }
            }

            // AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// SpineイベントのInt/Float/Stringすべてのパラメータを
        /// AnimationClipのAnimationEventに反映する
        /// </summary>
        private static void ImportAllSpineEventParameters(SkeletonDataAsset sda)
        {
            if (sda == null || sda.controller == null)
                return;

            var animatorController = sda.controller as UnityEditor.Animations.AnimatorController;
            if (animatorController == null)
                return;

            // SkeletonDataからSpineアニメーションとイベントデータを取得
            var skeletonData = sda.GetSkeletonData(true);
            if (skeletonData == null)
                return;

            // controllerAsset配下の全AnimationClipを取得
            var asset = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(animatorController));
            AnimationClip[] clips = System.Array.FindAll(asset, obj => obj is AnimationClip).Cast<AnimationClip>().ToArray();

            foreach (var clip in clips)
            {
                // Spineアニメーションを名前で検索
                var spineAnim = skeletonData.Animations.Items.FirstOrDefault(a => a.Name == clip.name);
                if (spineAnim == null)
                    continue;

                // 既存のイベントを取得
                var existingEvents = AnimationUtility.GetAnimationEvents(clip);
                var newEvents = new List<AnimationEvent>();

                // SpineアニメーションのタイムラインからEventTimelineを探索
                foreach (var timeline in spineAnim.Timelines)
                {
                    if (timeline is EventTimeline eventTimeline)
                    {
                        // EventTimelineから全イベントを取得
                        for (int i = 0; i < eventTimeline.Events.Length; i++)
                        {
                            Spine.Event spineEvent = eventTimeline.Events[i];
                            float time = eventTimeline.Frames[i];

                            // 新しいAnimationEventを作成
                            var animEvent = new AnimationEvent
                            {
                                time = time,
                                functionName = "OnSpineEvent", // 共通の受信メソッド名

                                // Spineイベントの全パラメータを設定
                                intParameter = spineEvent.Int,
                                floatParameter = spineEvent.Float,
                                // stringParameter = $"{spineEvent.Data.Name}[{spineEvent.String}]",

                                // 受診先なくてもよいに設定
                                messageOptions = SendMessageOptions.DontRequireReceiver
                            };

                            if (string.IsNullOrEmpty(spineEvent.String))
                            {
                                // "イベント名"
                                animEvent.stringParameter = spineEvent.Data.Name;
                            }
                            else
                            {
                                // "イベント名[文字列パラメータ]"
                                animEvent.stringParameter = $"{spineEvent.Data.Name}[{spineEvent.String}]";
                            }

                            newEvents.Add(animEvent);
                        }
                    }
                }

                // イベントを更新
                if (newEvents.Count > 0)
                {
                    AnimationUtility.SetAnimationEvents(clip, newEvents.ToArray());
                    EditorUtility.SetDirty(clip);
                    Debug.Log($"[SpineEventImporter] Updated {newEvents.Count} events for {clip.name}");
                }
            }

            // AssetDatabase.SaveAssets();
        }
    }
}