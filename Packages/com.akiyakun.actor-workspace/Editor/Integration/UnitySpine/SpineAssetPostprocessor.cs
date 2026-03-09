#nullable enable
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
using afl.Editor;

namespace ActorWorkspace.Editor.UnitySpine
{
    // インポート処理の設定はScriptableObjectから取得しています(UnitySpineSettings.csを参照)
    //
    // * SpineExtraDataファイルが存在しているかで新規インポートか更新かを判定しています
    //
    public class SpineAssetPostprocessor : AssetPostprocessor
    {
        [MenuItem("CONTEXT/SkeletonDataAsset/AW再インポート", false, 0)]
        static void MenuReImport(MenuCommand menuCommand)
        {
            reImportGard = true;
            targetList = new()
            {
                new AssetInfo(AssetDatabase.GetAssetPath(menuCommand.context), false)
            };
            OnDelayCall();
            Debug.Log("[SpineAssetPostprocessor] AW再インポート完了");
        }

        [MenuItem("CONTEXT/SkeletonDataAsset/AW更新インポート", false, 1)]
        static void MenuUpdateImport(MenuCommand menuCommand)
        {
            reImportGard = true;
            targetList = new()
            {
                new AssetInfo(AssetDatabase.GetAssetPath(menuCommand.context), true)
            };
            OnDelayCall();
            Debug.Log("[SpineAssetPostprocessor] AW更新インポート完了");
        }

        public static void ReImport(List<string> paths, bool isUpdate = true)
        {
            reImportGard = true;
            targetList = new();
            foreach (var path in paths)
            {
                targetList.Add(new AssetInfo(path, isUpdate));
            };
            OnDelayCall();
            Debug.Log("[SpineAssetPostprocessor] 再インポート完了");
        }

        class AssetInfo
        {
            public string Path;
            public bool IsUpdate;

            public AssetInfo(string path, bool isUpdate)
            {
                Path = path;
                IsUpdate = isUpdate;
            }
        }
        static List<AssetInfo>? targetList;
        static bool reImportGard = false;


        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (reImportGard) return;
            reImportGard = true;

            try
            {
                targetList = new();

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
                        string skeletonDataPath = SpineUtilityEditor.GetSkeletonDataPath(path);
                        targetList.Add(new AssetInfo(skeletonDataPath, SpineUtilityEditor.IsSpineExtraDataExists(skeletonDataPath)));
                        Debug.Log($"[SpineAssetPostprocessor] Imported Spine SkeletonDataAsset(.json): {skeletonDataPath}");

                        continue;
                    }

                    // 新規インポートかチェック
                    if (path.EndsWith(".asset") == true
                        && AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(SkeletonDataAsset)
                        // SpineExtraDataファイルが存在する場合は新規とみなさない
                        && SpineUtilityEditor.IsSpineExtraDataExists(path) == false)
                    {
                        targetList.Add(new AssetInfo(path, false));
                        Debug.Log($"[SpineAssetPostprocessor] Imported Spine SkeletonDataAsset(.asset): {path}");
                        continue;
                    }


                    // アセットのタイプでフィルタリング

                    //*/
                }
            }
            finally
            {
                if (targetList != null && targetList.Count > 0)
                {
                    EditorApplication.delayCall -= OnDelayCall;
                    EditorApplication.delayCall += OnDelayCall;
                }
                else
                {
                    targetList = null;
                    reImportGard = false;
                }
            }

        }

        static void OnDelayCall()
        {
            ProcessPendingAssetsAsync().Forget();
        }

        static async UniTask ProcessPendingAssetsAsync()
        {
            // EditorUtility.DisplayProgressBar("Spine", "Importing files...", 0f);
            // await UniTask.Delay(5000, DelayType.Realtime);

            try
            {
                if (targetList == null) return;

                float span = 1.0f / targetList.Count;
                float progress = 0.0f;
                foreach (var assetInfo in targetList)
                {
                    EditorUtility.DisplayProgressBar("Spine", $"Importing files...\n{assetInfo.Path}", progress);
                    await Process(assetInfo);
                    progress += span;
                }
            }
            finally
            {
                AssetDatabase.SaveAssets();

                targetList?.Clear();
                reImportGard = false;

                // 必ずプログレスバーをクリア
                EditorUtility.ClearProgressBar();

                Debug.Log("[SpineAssetPostprocessor] Spineのインポート処理が終了しました");
            }
        }

        static bool ErrorCheck(AssetInfo assetInfo)
        {
            var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(assetInfo.Path);
            if (skeletonDataAsset == null) return false;

            var ret = skeletonDataAsset.GetSkeletonData(false);
            if (ret == null)
            {
                // Debug.LogError($"[SpineAssetPostprocessor] Failed to get SkeletonData for path: {assetInfo.Path}");
                return false;
            }

            return true;
        }

        static async UniTask Process(AssetInfo assetInfo)
        {
            // 追加情報用ScriptableObjectの作成・取得と内容クリア
            SpineExtraDataScriptableObject spineExtraData = CreateSpineExtraDataAsset(assetInfo.Path, out bool isCreated);

            // エラーや警告が出ていないかチェック
            if (ErrorCheck(assetInfo) == false)
            {
                spineExtraData.IsImportError = true;
                EditorUtility.SetDirty(spineExtraData);
                AssetDatabase.SaveAssets();
                return;
            }

            // Spineデータに変更があった場合にAnimationClipを再生成する(確実にするため)
            SpineUtilityEditor.GenerateMecanimAnimationClip(assetInfo.Path);
            await EUtility.WaitForEditor();

            // Debug.Log($"[SpineAssetPostprocessor] Processing Spine Asset: {path}");
            var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(assetInfo.Path);
            if (skeletonDataAsset == null) return;

            // AnimationController が存在する場合
            // MEMO: 通常のインポートだけではcontrollerは自動生成されない
            if (skeletonDataAsset.controller != null)
            {
                SpineUtilityEditor.PreSetupAnimatorController(skeletonDataAsset);
            }

            ProcessFolders(skeletonDataAsset, spineExtraData);

            // SpineイベントのInt/Float/Stringすべてをインポート
            ImportAllSpineEventParameters(skeletonDataAsset, spineExtraData);

            // コールバック
            if (UnitySpineSettings.Instance.ImportCallback != null)
            {
                // 更新時
                if (assetInfo.IsUpdate == true)
                {
                    await UnitySpineSettings.Instance.ImportCallback.OnUpdateImported(assetInfo.Path, skeletonDataAsset, spineExtraData);
                }
                // 新規作成時
                else
                {
                    await UnitySpineSettings.Instance.ImportCallback.OnNewImported(assetInfo.Path, skeletonDataAsset, spineExtraData);
                }
            }

            spineExtraData.IsImportError = false;
            EditorUtility.SetDirty(spineExtraData);

            AssetDatabase.SaveAssets();

            await EUtility.WaitForEditor();
        }

        // 追加情報用ScriptableObjectの作成・取得と内容クリア
        // isCreated: 新規作成された場合true
        static SpineExtraDataScriptableObject CreateSpineExtraDataAsset(string skeletonDataAssetPath, out bool isCreated)
        {
            string extraDataPath = SpineUtilityEditor.GetSpineExtraDataPath(skeletonDataAssetPath);
            SpineExtraDataScriptableObject ret = null!;

            if (File.Exists(extraDataPath) == true)
            {
                ret = AssetDatabase.LoadAssetAtPath<SpineExtraDataScriptableObject>(extraDataPath);
                isCreated = false;
            }
            else
            {
                ret = ScriptableObject.CreateInstance<SpineExtraDataScriptableObject>();
                AssetDatabase.CreateAsset(ret, extraDataPath);
                // AssetDatabase.SaveAssets();
                isCreated = true;
            }

            if (ret == null) throw new System.Exception($"Failed to create or load SpineExtraData at path: {extraDataPath}");
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
#nullable restore