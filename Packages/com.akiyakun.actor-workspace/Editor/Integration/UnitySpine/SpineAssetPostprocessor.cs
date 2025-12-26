using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using System.Linq;
using System;
using ActorWorkspace;
using ActorWorkspace.UnitySpine;
using afl;

namespace ActorWorkspace.Editor.UnitySpine
{
    public class SpineAssetPostprocessor : AssetPostprocessor
    {
        [MenuItem("CONTEXT/SkeletonDataAsset/AW再インポート", false, 0)]
        static void ReImport(MenuCommand menuCommand)
        {
            reImportGard = true;
            targetList = new List<string>();
            targetList.Add(AssetDatabase.GetAssetPath(menuCommand.context));
            ProcessPendingAssets();
        }

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

        // AnimationClipの再生成
        // Spineデータに変更があった場合にAnimationClipを再生成する(確実にするため)
        // See also: https://ja.esotericsoftware.com/forum/d/14630-c-force-update-animationclips
        static void GenerateMecanimAnimationClips(List<string> targets)
        {
            foreach (var path in targets)
            {
                var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
                if (skeletonDataAsset == null || skeletonDataAsset.controller == null) continue;

                D.LogVerbose($"[SpineMecanimPostprocessor] Updating Mecanim AnimationClips: {skeletonDataAsset.name}");

                // MEMO: GenerateMecanimAnimationClips()内でSaveAssetes()している
                SkeletonBaker.GenerateMecanimAnimationClips(skeletonDataAsset);
            }
        }

        static void ProcessPendingAssets()
        {
            // Debug.Log($"3 cout={targetList.Count}");

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
                // Debug.Log($"[SpineAssetPostprocessor] Processing Spine Asset: {path}");
                var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
                if (skeletonDataAsset == null) continue;

                // 追加情報用ScriptableObjectの作成・取得と内容クリア
                SpineExtraDataScriptableObject spineExtraData = CreateSpineExtraDataAsset(path);
                EditorUtility.SetDirty(spineExtraData);

                // SpineイベントのInt/Float/Stringすべてをインポート
                ImportAllSpineEventParameters(skeletonDataAsset, spineExtraData);

                // AnimationController が存在する場合
                if (skeletonDataAsset.controller != null)
                {
                    // "_loop"サフィックスのAnimationClipをループ設定にする
                    SetLoopForLoopSuffix(skeletonDataAsset);
                }

                ProcessFolders(skeletonDataAsset, spineExtraData);

            }
        }

        // 追加情報用ScriptableObjectの作成・取得と内容クリア
        public static SpineExtraDataScriptableObject CreateSpineExtraDataAsset(string skeletonDataAssetPath)
        {
            string name = Path.GetFileNameWithoutExtension(skeletonDataAssetPath).Replace("_SkeletonData", "");
            string extraDataPath = $"{Path.GetDirectoryName(skeletonDataAssetPath)}/{name}_SpineExtraData.asset";

            SpineExtraDataScriptableObject ret = null;

            if (File.Exists(extraDataPath) == true)
            {
                ret = AssetDatabase.LoadAssetAtPath<SpineExtraDataScriptableObject>(extraDataPath);
            }
            else
            {
                ret = ScriptableObject.CreateInstance<SpineExtraDataScriptableObject>();
                AssetDatabase.CreateAsset(ret, extraDataPath);
                AssetDatabase.SaveAssets();
            }

            Debug.Assert(ret != null);
            ret.Clear();

            return ret;
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
                        // Debug.Log($"[SpineLoopClipSetter] Set loopTime = true for {clip.name}");
                    }
                }
            }

            // AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// SpineイベントのInt/Float/Stringすべてのパラメータを
        /// AnimationClipのAnimationEventに反映する
        /// </summary>
        private static void ImportAllSpineEventParameters(SkeletonDataAsset sda, SpineExtraDataScriptableObject spineExtraData)
        {
            if (sda == null || sda.controller == null) return;

            var animatorController = sda.controller as UnityEditor.Animations.AnimatorController;
            if (animatorController == null) return;

            // SkeletonDataからSpineアニメーションとイベントデータを取得
            var skeletonData = sda.GetSkeletonData(true);
            if (skeletonData == null) return;

            // controllerAsset配下の全AnimationClipを取得
            var asset = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(animatorController));
            AnimationClip[] clips = System.Array.FindAll(asset, obj => obj is AnimationClip).Cast<AnimationClip>().ToArray();

            foreach (var clip in clips)
            {
                // Spineアニメーションを名前で検索
                var spineAnim = skeletonData.Animations.Items.FirstOrDefault(a => a.Name == clip.name);
                if (spineAnim == null) continue;

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

                                // MonoBehaviour に同名のメソッドを実装してイベントを受け取る仕様のため、
                                // 共通の受信メソッド名を使い stringParameter にイベント名を入れています
                                functionName = "OnSpineEvent",

                                // Spineイベントの全パラメータを設定
                                intParameter = spineEvent.Int,
                                floatParameter = spineEvent.Float,
                                // stringParameter = $"{spineEvent.Data.Name}[{spineEvent.String}]",

                                // 受診先なくてもよいに設定
                                messageOptions = SendMessageOptions.DontRequireReceiver
                            };

                            if (string.IsNullOrEmpty(spineEvent.Data.AudioPath) == false)
                            {
                                // AudioPathが設定されている場合は優先的に設定
                                // animEvent.stringParameter = spineEvent.Data.AudioPath;
                                animEvent.stringParameter = AWDefaultAnimationEvents.Audio.ToString();

                                var filename = Path.GetFileNameWithoutExtension(spineEvent.Data.AudioPath);
                                animEvent.intParameter = int.Parse(filename.AsSpan(0, 4));

                                // MEMO: ボリュームとバランスのパラメータにまだ非対応
                            }
                            // Stringパラメータが空の場合は追加情報なしと判断
                            else if (string.IsNullOrEmpty(spineEvent.String))
                            {
                                // "イベント名"
                                // MEMO: タイムラインでキーを打っていないイベントは除かれるようです

                                // SpineEvent名をAnimationEventのStringパラメータにする
                                string eventName = spineEvent.Data.Name;
                                Debug.Log($"SpineEvent Name: {eventName}");

                                // 特殊記号の処理
                                if (eventName.IndexOf('+') >= 0)
                                {
                                    spineExtraData.AddAttachmentName(
                                        SpineExtraDataScriptableObject.EffectBoneFollower, eventName, alertAlreadyExist: false);
                                }
                                else if (eventName.IndexOf('*') >= 0)
                                {
                                    spineExtraData.AddAttachmentName(
                                        SpineExtraDataScriptableObject.EffectPointFollower, eventName, alertAlreadyExist: false);
                                }

                                animEvent.stringParameter = eventName;
                            }
                            // Stringパラメータがある場合
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
                    // Debug.Log($"[SpineEventImporter] Updated {newEvents.Count} events for {clip.name}");
                }
            }

            // AssetDatabase.SaveAssets();
        }

        private static void ProcessFolders(SkeletonDataAsset sda, SpineExtraDataScriptableObject spineExtraData)
        {
            var skeletonData = sda.GetSkeletonData(true);
            if (skeletonData == null) return;

            var folderNames = new List<string>()
            {
                SpineExtraDataScriptableObject.CollisionBoxFollower,
                SpineExtraDataScriptableObject.HurtBoxFollower,
                SpineExtraDataScriptableObject.HitBoxFollower,
            };

            foreach (var name in folderNames)
            {
                var slotDataList = SpineUtilityEditor.GetSlotsUnderBone(skeletonData, name);
                foreach (var slotData in slotDataList)
                {
                    spineExtraData.AddAttachmentName(name, slotData.Name);
                }
            }
        }
    }
}