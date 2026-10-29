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
            var targetList = new List<AssetInfo>()
            {
                new AssetInfo(AssetDatabase.GetAssetPath(menuCommand.context), false)
            };
            OnDelayCall(targetList);
            Debug.Log("[SpineAssetPostprocessor] AW再インポート完了");
        }

        [MenuItem("CONTEXT/SkeletonDataAsset/AW更新インポート", false, 1)]
        static void MenuUpdateImport(MenuCommand menuCommand)
        {
            var targetList = new List<AssetInfo>()
            {
                new AssetInfo(AssetDatabase.GetAssetPath(menuCommand.context), true)
            };
            OnDelayCall(targetList);
            Debug.Log("[SpineAssetPostprocessor] AW更新インポート完了");
        }

        public static void ReImport(List<string> paths, bool isUpdate = true)
        {
            var targetList = new List<AssetInfo>();
            foreach (var path in paths)
            {
                targetList.Add(new AssetInfo(path, isUpdate));
            };
            OnDelayCall(targetList);
            Debug.Log("[SpineAssetPostprocessor] 再インポート完了");
        }

        // SpineExtraDataの有無で更新/新規を判定する
        // (.jsonインポート時・一括再インポート時・メニュー表示で共通のルール)
        public static bool IsUpdateImport(string skeletonDataAssetPath)
        {
            return SpineUtilityEditor.IsSpineExtraDataExists(skeletonDataAssetPath);
        }

        // 更新/新規をパスごとに自動判定し、1バッチで再インポートする
        public static void ReImportAuto(IEnumerable<string> paths)
        {
            var targetList = new List<AssetInfo>();
            foreach (var path in paths)
            {
                targetList.Add(new AssetInfo(path, IsUpdateImport(path)));
            }
            if (targetList.Count == 0) return;

            OnDelayCall(targetList);
            Debug.Log($"[SpineAssetPostprocessor] 再インポートを開始: {targetList.Count} 件");
        }

        [Serializable]
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

        // 処理待ちリストの保存用(JsonUtilityはListを直接シリアライズできないため)
        [Serializable]
        class PendingList
        {
            public List<AssetInfo> Items = new();
        }

        // 処理待ちリストをドメインリロードを跨いで保持するためのSessionStateキー
        // (エディタ終了時には自動でクリアされる)
        const string PendingListSessionKey = "ActorWorkspace.SpineAssetPostprocessor.PendingList";

        // List<AssetInfo>? targetList;
        // static List<string>? extraJsonList;
        static int reImportGard;


        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // if (reImportGard) return;
            // reImportGard++;

            List<AssetInfo> targetList = new();

            try
            {
                // targetList = new();
                // extraJsonList = new();

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
                        targetList.Add(new AssetInfo(skeletonDataPath, IsUpdateImport(skeletonDataPath)));
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

                    /* エクスポートしたときSpineの.jsonに日付時間を付与するようにしたので必要なくなった
                    // extra_data.jsonのみ更新された場合の検知
                    if (path.EndsWith(SpineUtilityEditor.ExtraDataJsonFileName) == true)
                    {
                        // FIXME: 同フォルダ内のSkeletonDataAssetを探す必要がある
                        // 検知用に一旦そのままパスを入れる
                        extraJsonList.Add(path);

                        // SkeletonDataAssetが更新されたことにするためこちらのパスをAddする
                        // string skeletonDataPath = SpineUtilityEditor.GetSkeletonDataPath(path);
                        // targetList.Add(new AssetInfo(skeletonDataPath, SpineUtilityEditor.IsSpineExtraDataExists(skeletonDataPath)));
                        // Debug.Log($"[SpineAssetPostprocessor] Imported Spine ExtraData JSON: {skeletonDataPath}");
                        continue;
                    }
                    */
                }// foreach

                /*
                // extra_data.jsonのみ更新された場合の検知
                if (targetList.Count == 0 && extraJsonList.Count > 0)
                {
                    foreach (var extraJsonPath in extraJsonList)
                    {
                        Debug.LogError($"[SpineAssetPostprocessor] extra_data.jsonのみ変更した場合、更新インポートされないので.spineファイルに何かしら変更を加えて更新が検知されるようにしてもう一度インポートを行なってください。 Path: {extraJsonPath}");
                    }
                }
                */
            }
            finally
            {
                if (targetList.Count > 0)
                {
                    OnDelayCall(targetList);
                }
            }

        }

        static void OnDelayCall(List<AssetInfo> targetList)
        {
            AddPendingList(targetList);
            ProcessPendingAssetsAsync().Forget();
        }

        // ドメインリロードで消えたインポートキューを再開する
        // (初回起動時の一括インポートや処理中の再コンパイルでドメインリロードが起きると、
        //  実行中のUniTaskが消滅して残りのアセットが未変換のまま残るため)
        [InitializeOnLoadMethod]
        static void ResumePendingAssets()
        {
            if (LoadPendingList().Count == 0) return;

            // UniTaskのエディタループ初期化より先に呼ばれる可能性があるためdelayCallで遅らせる
            EditorApplication.delayCall += () =>
            {
                // Play Mode突入時のドメインリロードでは再開しない(終了時のリロードで改めて再開される)
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;

                var pendingList = LoadPendingList();
                if (pendingList.Count == 0) return;

                Debug.Log($"[SpineAssetPostprocessor] 中断されたSpineインポートを再開: {pendingList.Count} 件");
                ProcessPendingAssetsAsync().Forget();
            };
        }

        static List<AssetInfo> LoadPendingList()
        {
            var json = SessionState.GetString(PendingListSessionKey, "");
            if (string.IsNullOrEmpty(json)) return new List<AssetInfo>();

            try
            {
                var pendingList = JsonUtility.FromJson<PendingList>(json);
                return pendingList?.Items ?? new List<AssetInfo>();
            }
            catch (Exception e)
            {
                // 壊れた保存データでセッション中ずっと例外が出続けないよう、読めなければ破棄する
                Debug.LogError($"[SpineAssetPostprocessor] 処理待ちリストの読み込みに失敗したため破棄します: {e.Message}");
                SessionState.EraseString(PendingListSessionKey);
                return new List<AssetInfo>();
            }
        }

        static void SavePendingList(List<AssetInfo> list)
        {
            if (list.Count == 0)
            {
                SessionState.EraseString(PendingListSessionKey);
                return;
            }
            SessionState.SetString(PendingListSessionKey, JsonUtility.ToJson(new PendingList() { Items = list }));
        }

        // 処理待ちリストへ追加して保存
        static void AddPendingList(List<AssetInfo> targetList)
        {
            var pendingList = LoadPendingList();
            pendingList.AddRange(targetList);
            SavePendingList(pendingList);
        }

        static async UniTask ProcessPendingAssetsAsync()
        {
            // EditorUtility.DisplayProgressBar("Spine", "Importing files...", 0f);

            await EUtility.WaitForEditor();

            // コンパイル・インポート中はアセットのロードに失敗することがあるため待機
            while (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                await UniTask.Yield();
            }

            // 前のForget処理が完了するまでビジーループで待機
            while (reImportGard != 0)
            {
                await UniTask.Yield();
            }
            reImportGard++;


            try
            {
                int processedCount = 0;
                var attemptCounts = new Dictionary<string, int>();
                while (true)
                {
                    // 実行中に追加された分も含めて、毎回SessionStateから取り直す
                    var pendingList = LoadPendingList();
                    if (pendingList.Count == 0) break;

                    var assetInfo = pendingList[0];

                    // 処理中の再インポートで同じアセットが積み直され続けた場合の安全弁
                    int attempts = attemptCounts.TryGetValue(assetInfo.Path, out var count) ? count + 1 : 1;
                    attemptCounts[assetInfo.Path] = attempts;
                    if (attempts > 3)
                    {
                        Debug.LogError($"[SpineAssetPostprocessor] 同じアセットが繰り返し積まれるため処理を打ち切ります: {assetInfo.Path}");
                        pendingList.RemoveAll(t => t.Path == assetInfo.Path);
                        SavePendingList(pendingList);
                        continue;
                    }

                    float progress = (float)processedCount / (processedCount + pendingList.Count);
                    EditorUtility.DisplayProgressBar("Spine", $"Importing files...\n{assetInfo.Path}", progress);

                    // 1件の失敗でバッチの残りを取りこぼさないよう、アセット単位で例外を握って次へ進む
                    // (Spine側のクリップ再生成だけが走り、イベントがSpine標準形式のまま残るのを防ぐ)
                    try
                    {
                        await Process(assetInfo);
                    }
                    catch (Exception e)
                    {
                        // エラーフラグは Process() 側で管理する(コア処理が完了していれば解除済み)
                        var context = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetInfo.Path);
                        Debug.LogError($"[SpineAssetPostprocessor] Import failed: {assetInfo.Path}", context);
                        Debug.LogException(e, context);
                    }

                    // 処理済み(失敗含む)の1件を除去して保存
                    // (途中でドメインリロードが起きた場合は、残りが ResumePendingAssets() から再開される)
                    pendingList = LoadPendingList();
                    int index = pendingList.FindIndex(t => t.Path == assetInfo.Path);
                    if (index >= 0) pendingList.RemoveAt(index);
                    SavePendingList(pendingList);

                    processedCount++;
                }
            }
            finally
            {
                AssetDatabase.SaveAssets();

                reImportGard--;

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

            // コア処理(クリップ再生成・イベント変換・Folder収集)が終わるまではエラー扱いにしておく
            // 途中で例外が出た場合はこの状態のまま保存され、実行時に SetupForExtraData が検知する
            spineExtraData.IsImportError = true;
            EditorUtility.SetDirty(spineExtraData);

            // エラーや警告が出ていないかチェック
            if (ErrorCheck(assetInfo) == false)
            {
                AssetDatabase.SaveAssets();
                Debug.LogError($"[SpineAssetPostprocessor] Import error. {assetInfo.Path}");
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

            // ここまで成功すればゲームから使える状態なのでエラーを解除する
            // プロジェクト側コールバックの失敗はログに出すだけで、エラーフラグには反映しない
            // (例: Enemy 専用の処理が Enemy 以外のアセットで失敗しても、そのアセットが実行時に使えなくなるのを防ぐ)
            spineExtraData.IsImportError = false;
            EditorUtility.SetDirty(spineExtraData);

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

                // コールバック中のawaitで保留中のインポートが走ると、SetDirty済みの状態が先に書き出されて
                // dirtyフラグが消え、コールバックによる変更(DefaultApplyRootMotion等)が保存されないことがあるため、
                // コールバック後に改めてSetDirtyする
                EditorUtility.SetDirty(spineExtraData);
            }

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

            bool hasChanges = false;
            foreach (var clip in clips)
            {
                // Spineアニメーションを名前で検索
                var spineAnim = skeletonData.Animations.Items.FirstOrDefault(a => a.Name == clip.name);
                if (spineAnim == null) continue;

                // 既存のイベントを取得
                // var existingEvents = AnimationUtility.GetAnimationEvents(clip);
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
                                animEvent.stringParameter = AWCoreAnimationEvents.Audio.ToString();

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

                // イベントを更新（イベントが0件になった場合もクリアするため条件なしで常に呼ぶ）
                AnimationUtility.SetAnimationEvents(clip, newEvents.ToArray());
                EditorUtility.SetDirty(clip);
                hasChanges = true;
                // Debug.Log($"[SpineEventImporter] Updated {newEvents.Count} events for {clip.name}");
            }

            // AnimationClipはcontrollerファイルのサブアセットのため、
            // clipだけでなく親のanimatorControllerもSetDirtyしないと.controllerファイルが保存されない
            if (hasChanges)
            {
                EditorUtility.SetDirty(animatorController);
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