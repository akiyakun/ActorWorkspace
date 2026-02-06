using System.IO;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using System.Linq;
using System;
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
            Debug.Log("[SpineAssetPostprocessor] AW再インポート完了");
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
                    // インポートターゲットディレクトリか？
                    if (UnitySpineSettings.Instance.CheckTargetDirectory(path) == false) continue;

                    // MEMO: データ更新時はSkeletonDataAssetの更新が検知されないので.jsonファイルを監視する必要がある
                    if (path.EndsWith(".json") == true
                        && AssetUtility.CheckForValidSkeletonData(path) == true)
                    {
                        // Debug.Log($"[SpineAssetPostprocessor] Imported Spine JSON: {path}");
                        // SkeletonDataAssetが更新されたことにするためこちらのパスをAddする
                        string skeletonDataPath = $"{Utility.GetPathWithoutExtension(path)}{AssetUtility.SkeletonDataSuffix}.asset";
                        targetList.Add(skeletonDataPath);
                        Debug.Log($"[SpineAssetPostprocessor] Imported Spine SkeletonDataAsset(.json): {skeletonDataPath}");

                        continue;
                    }

                    // 新規インポートかチェック
                    if (path.EndsWith(".asset") == true
                        && AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(SkeletonDataAsset)
                        // SpineExtraDataファイルが存在する場合は新規とみなさない
                        && IsSpineExtraDataExists(path) == false)
                    {
                        targetList.Add(path);
                        Debug.Log($"[SpineAssetPostprocessor] Imported Spine SkeletonDataAsset(.asset): {path}");
                        continue;
                    }


                    // アセットのタイプでフィルタリング

                    //*/
                }
            }
            finally
            {
                if (targetList.Count > 0)
                {
                    // 先にクリップを再生成しておく
                    GenerateMecanimAnimationClips(targetList);

                    // 1フレーム後に実行
                    // MEMO: Spineライブラリ側の.json のインポート処理が完了していないため
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

        // Spineデータに変更があった場合にAnimationClipを再生成する(確実にするため)
        static void GenerateMecanimAnimationClips(List<string> targets)
        {
            foreach (var path in targets)
            {
                SpineUtilityEditor.GenerateMecanimAnimationClip(path);
            }
        }

        static void ProcessPendingAssets()
        {
            ProcessPendingAssetsAsync().Forget();
        }

        static async UniTask ProcessPendingAssetsAsync()
        {
            // Debug.Log($"3 cout={targetList.Count}");

            // EditorUtility.DisplayProgressBar("処理中", "しばらくお待ちください...", 0f);
            // await UniTask.Delay(5000, DelayType.Realtime);

            try
            {
                await Process(targetList);

                targetList.Clear();

                // AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

            }
            finally
            {
                reImportGard = false;

                // 必ずプログレスバーをクリア
                // EditorUtility.ClearProgressBar();

                Debug.Log("[SpineAssetPostprocessor] Spineのインポート処理が終了しました");
            }
        }

        static async UniTask Process(List<string> targets)
        {
            foreach (var path in targets)
            {
                // EditorUtility.DisplayProgressBar("処理中", "もう少しです...1", 0.3f);

                // Debug.Log($"[SpineAssetPostprocessor] Processing Spine Asset: {path}");
                var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
                if (skeletonDataAsset == null) continue;

                // AnimationController が存在する場合
                // MEMO: 通常のインポートだけではcontrollerは自動生成されない
                if (skeletonDataAsset.controller != null)
                {
                    SpineUtilityEditor.PreSetupAnimatorController(skeletonDataAsset);
                }

                // 追加情報用ScriptableObjectの作成・取得と内容クリア
                SpineExtraDataScriptableObject spineExtraData = CreateSpineExtraDataAsset(path, out bool isCreated);
                EditorUtility.SetDirty(spineExtraData);

                ProcessFolders(skeletonDataAsset, spineExtraData);

                // SpineイベントのInt/Float/Stringすべてをインポート
                ImportAllSpineEventParameters(skeletonDataAsset, spineExtraData);

                // 新規作成時のコールバック
                if (isCreated == true
                    && UnitySpineSettings.Instance.ImportCallback != null)
                {
                    await UnitySpineSettings.Instance.ImportCallback.OnNewImported(path, skeletonDataAsset, spineExtraData);
                }

                // EditorUtility.DisplayProgressBar("処理中", "もう少しです...2", 0.3f);

                // await UniTask.Yield();
                // await UniTask.WaitForSeconds(4.0f, ignoreTimeScale: true);
                await UniTask.Delay(10, DelayType.Realtime);

            }
        }

        static string GetSpineExtraDataPath(string skeletonDataAssetPath)
        {
            string name = Path.GetFileNameWithoutExtension(skeletonDataAssetPath).Replace("_SkeletonData", "");
            return $"{Path.GetDirectoryName(skeletonDataAssetPath)}/{name}_SpineExtraData.asset";
        }

        static bool IsSpineExtraDataExists(string skeletonDataAssetPath)
        {
            return File.Exists(GetSpineExtraDataPath(skeletonDataAssetPath));
        }

        // 追加情報用ScriptableObjectの作成・取得と内容クリア
        // isCreated: 新規作成された場合true
        static SpineExtraDataScriptableObject CreateSpineExtraDataAsset(string skeletonDataAssetPath, out bool isCreated)
        {
            string extraDataPath = GetSpineExtraDataPath(skeletonDataAssetPath);
            SpineExtraDataScriptableObject ret = null;

            if (File.Exists(extraDataPath) == true)
            {
                ret = AssetDatabase.LoadAssetAtPath<SpineExtraDataScriptableObject>(extraDataPath);
                isCreated = false;
            }
            else
            {
                ret = ScriptableObject.CreateInstance<SpineExtraDataScriptableObject>();
                AssetDatabase.CreateAsset(ret, extraDataPath);
                AssetDatabase.SaveAssets();
                isCreated = true;
            }

            Debug.Assert(ret != null);
            ret.Clear();
            Debug.Log($"[SpineAssetPostprocessor] Created/Loaded SpineExtraData: {extraDataPath}");

            return ret;
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
                                try
                                {
                                    animEvent.intParameter = int.Parse(filename.AsSpan(0, 4));
                                }
                                catch (Exception)
                                {
                                    D.LogError($"[SpineEventImporter] Failed from AudioPath: {spineEvent.Data.AudioPath}");
                                }

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
                                    // spineExtraData.AddAttachment(
                                    //     UnitySpineSettings.Instance.EffectBoneFollower, SpineNodeType.Bone, eventName, alertAlreadyExist: false);
                                    if (spineExtraData.GetAttachment(UnitySpineSettings.Instance.EffectBoneFollower, eventName) == null)
                                    {
                                        throw new Exception($"SpineExtraData does not contain attachment for BoneFollower: {eventName}");
                                    }
                                }
                                else if (eventName.IndexOf('*') >= 0)
                                {
                                    // spineExtraData.AddAttachment(
                                    //     UnitySpineSettings.Instance.EffectPointFollower, SpineNodeType.Bone, eventName, alertAlreadyExist: false);
                                    if (spineExtraData.GetAttachment(UnitySpineSettings.Instance.EffectPointFollower, eventName) == null)
                                    {
                                        throw new Exception($"SpineExtraData does not contain attachment for PointFollower: {eventName}");
                                    }
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

            foreach (var folderSetting in UnitySpineSettings.Instance.FolderSettings)
            {
                // ボーンを使用しないといけないのは BoundingBoxFollower 以外のタイプ
                // if (folderSetting.FollowerType != UnitySpineSettings.FollowerType.BoundingBoxFollower)
                {
                    var boneDataList = SpineUtilityEditor.GetBonesUnderBone(skeletonData, folderSetting.FolderName, depth: 1);
                    foreach (var boneData in boneDataList)
                    {
                        spineExtraData.AddAttachment(folderSetting.FolderName, SpineNodeType.Bone, boneData.Name);
                    }
                }

                // スロットを使用できるのは BoundingBoxFollower タイプのみ
                // if (folderSetting.FollowerType == UnitySpineSettings.FollowerType.BoundingBoxFollower)
                {
                    var slotDataList = SpineUtilityEditor.GetSlotsUnderBone(skeletonData, folderSetting.FolderName, depth: 1);
                    foreach (var slotData in slotDataList)
                    {
                        spineExtraData.AddAttachment(folderSetting.FolderName, SpineNodeType.Slot, slotData.Name);
                    }
                }

            }// foreach folderSetting
        }
    }
}