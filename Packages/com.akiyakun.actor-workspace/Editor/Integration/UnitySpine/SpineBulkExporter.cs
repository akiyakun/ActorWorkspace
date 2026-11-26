#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Spine.Unity;
using ActorWorkspace.UnitySpine;
using afl;
using afl.Editor;

namespace ActorWorkspace.Editor.UnitySpine
{
    // Spine の元データをすべて Unity 用に書き出す
    //
    // 1 件ずつ、次の順に処理する。エラーが出たら、その件で止める
    // 1. tools/spine/spine_actor_export_to_unity.py で書き出す(Python と Spine が要る)
    // 2. 取り込みが終わるのを待つ(SpineAssetPostprocessor の処理を含む)
    // 3. エラーが出ていないか確かめる(その間に出たエラーログ、SpineExtraData の IsImportError)
    //
    // 元データと書き出し先のフォルダは UnitySpineSettings の Export Settings で決める
    // MEMO: 途中でスクリプトが変わってドメインリロードが起きると、そこで黙って止まる(実行中はスクリプトを変えない)
    public static class SpineBulkExporter
    {
        const string MenuPath = PackageSetup.Develop_AW + "Spine/Export All Spine To Unity";
        const string LogPrefix = "[SpineBulkExporter]";
        const string ProgressTitle = "Spine の一括書き出し";

        // 書き出すフォルダの名前の末尾(アセットバンドルにするアクターのフォルダ)
        // MEMO: カットインなどの UI の Spine は、ここに当てはまらないフォルダに置いて対象から外す
        const string ExportFolderSuffix = "@bundle";

        // 書き出しのスクリプト(tools フォルダからの相対パス)
        const string ExporterScriptPath = "spine/spine_actor_export_to_unity.py";

        // 取り込みの処理が無い状態がこの秒数続いたら、取り込みが終わったとみなす
        // MEMO: 取り込みの後処理でアセットが作られ、取り込みが続けて起きることがあるため
        const double ImportIdleSeconds = 1.0;
        // 1 件の取り込みを待つ上限
        const double ImportTimeoutSeconds = 600.0;

        static bool isRunning;
        static bool isCancelRequested;

        [MenuItem(MenuPath, false, MenuItems.Priority.Develop + 10)]
        static void MenuExportAll() => ExportAllAsync().Forget();

        [MenuItem(MenuPath, true)]
        static bool MenuExportAllValidate() => isRunning == false;

        public static async UniTask<int> ExportAllAsync()
        {
            if (isRunning == true) return GeneralReturnCode.Failed;

            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                EditorUtility.DisplayDialog(ProgressTitle, "Play 中は書き出せません。Play を止めてから実行してください。", "OK");
                return GeneralReturnCode.Failed;
            }

            var settings = UnitySpineSettings.Instance;
            string sourceDirectory = Path.GetFullPath(Path.Combine(EUtility.GetRootPath(), settings.ExportSourceDirectory));
            string destinationDirectory = settings.ExportDestinationDirectory.TrimEnd('/');
            if (Directory.Exists(sourceDirectory) == false)
            {
                EditorUtility.DisplayDialog(ProgressTitle, $"Spine の元データのフォルダがありません。\n{sourceDirectory}\n\nUnitySpineSettings の Export Source Directory を確認してください。", "OK");
                return GeneralReturnCode.Failed;
            }

            var folders = FindExportFolders(sourceDirectory);
            if (folders.Count == 0)
            {
                EditorUtility.DisplayDialog(ProgressTitle, $"書き出すフォルダ(<カテゴリ>/<名前>{ExportFolderSuffix})がありません。\n{sourceDirectory}", "OK");
                return GeneralReturnCode.Failed;
            }

            string message = $"{folders.Count} 件の Spine を書き出して取り込みます。\n\n元データ: {sourceDirectory}\n書き出し先: {destinationDirectory}\n\nエラーが出たら、その件で止めます。";
            if (EditorUtility.DisplayDialog(ProgressTitle, message, "書き出す", "キャンセル") == false) return GeneralReturnCode.Canceled;

            isRunning = true;
            isCancelRequested = false;
            int ret = GeneralReturnCode.Succeeded;
            int exportedCount = 0;
            string? failedFolder = null;
            try
            {
                // 前の取り込みが残っていれば、先に終わらせる
                if (await WaitForImportAsync() < 0)
                {
                    failedFolder = "(書き出す前の取り込み)";
                    ret = GeneralReturnCode.Failed;
                }

                int count = folders.Count;
                for (int i = 0; i < count && ret >= 0; ++i)
                {
                    if (isCancelRequested == true)
                    {
                        ret = GeneralReturnCode.Canceled;
                        break;
                    }

                    try
                    {
                        ret = await ExportOneAsync(folders[i], sourceDirectory, destinationDirectory, i, count);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"{LogPrefix} {folders[i]} の書き出しで例外が出たので止めました");
                        Debug.LogException(e);
                        ret = GeneralReturnCode.Failed;
                    }
                    if (ret < 0)
                    {
                        failedFolder = folders[i];
                        break;
                    }
                    ++exportedCount;
                }

                // 最後の件の書き出し中にキャンセルされたときも、キャンセルとして伝える
                if (ret == GeneralReturnCode.Succeeded && isCancelRequested == true) ret = GeneralReturnCode.Canceled;
            }
            finally
            {
                isRunning = false;
                EditorUtility.ClearProgressBar();
            }

            // 結果
            if (ret == GeneralReturnCode.Succeeded)
            {
                Debug.Log($"{LogPrefix} {exportedCount} 件の Spine を書き出しました");
                EditorUtility.DisplayDialog(ProgressTitle, $"{exportedCount} 件の Spine を書き出しました。", "OK");
            }
            else if (ret == GeneralReturnCode.Canceled)
            {
                Debug.LogWarning($"{LogPrefix} キャンセルしました({exportedCount}/{folders.Count} 件を書き出し済み)");
                EditorUtility.DisplayDialog(ProgressTitle, $"キャンセルしました。\n{exportedCount}/{folders.Count} 件を書き出し済みです。", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(ProgressTitle, $"エラーが出たので止めました。\n{failedFolder}\n\n{exportedCount}/{folders.Count} 件を書き出し済みです。内容は Console を見てください。", "OK");
            }
            return ret;
        }

        // <元データのフォルダ>/<カテゴリ>/<名前>@bundle のフォルダを、名前の順に返す
        static List<string> FindExportFolders(string sourceDirectory)
        {
            var ret = new List<string>();
            foreach (var categoryDirectory in Directory.GetDirectories(sourceDirectory).OrderBy(x => x, System.StringComparer.Ordinal))
            {
                foreach (var directory in Directory.GetDirectories(categoryDirectory).OrderBy(x => x, System.StringComparer.Ordinal))
                {
                    if (Path.GetFileName(directory).EndsWith(ExportFolderSuffix) == false) continue;
                    ret.Add(directory);
                }
            }
            return ret;
        }

        static async UniTask<int> ExportOneAsync(string folder, string sourceDirectory, string destinationDirectory, int index, int count)
        {
            string folderName = Path.GetFileName(folder);
            string categoryName = Path.GetFileName(Path.GetDirectoryName(folder)!);
            string displayName = $"{categoryName}/{folderName}";
            // 書き出しのスクリプトは <書き出し先>/<カテゴリ>/<フォルダの名前> に書き出す
            string outputAssetPath = $"{destinationDirectory}/{categoryName}/{folderName}";
            string progressTitle = $"{ProgressTitle} ({index + 1}/{count})";

            var errors = new List<string>();
            var exporterOutput = new StringBuilder();
            using (new ErrorLogWatcher(errors))
            {
                // 書き出し先のフォルダの名前を合わせる(Unity の中で名前を変えるので、GUID が変わらず、Unity が落ちることもない)
                // MEMO: 書き出しのスクリプトも同じことをするが、Unity が開いているときは危ないので、先にここで変えておく
                RenameOutputFolder(destinationDirectory, categoryName, folderName, errors);

                // 書き出し
                int exitCode = -1;
                if (errors.Count == 0)
                {
                    EditorUtility.DisplayProgressBar(progressTitle, $"書き出し中: {displayName}", (float)index / count);
                    // 書き出しの間、取り込み・保存・ドメインリロードを止める(書き出しの途中のファイルを取り込まないように)
                    // 解除したときに取り込みが始まる
                    using (ExternalEditLock.Acquire())
                    {
                        exitCode = await RunExporterAsync(folder, Path.GetFullPath(EUtility.GetRootPath() + destinationDirectory), exporterOutput, progressTitle, displayName, (float)index / count);
                    }
                    if (exitCode != 0) errors.Add($"書き出しのスクリプトが失敗しました(終了コード {exitCode})");
                }

                if (errors.Count == 0)
                {
                    // 取り込み
                    EditorUtility.DisplayProgressBar(progressTitle, $"取り込み中: {displayName}", (float)index / count);
                    AssetDatabase.Refresh();
                    if (await WaitForImportAsync() < 0)
                    {
                        errors.Add($"取り込みが {ImportTimeoutSeconds} 秒で終わりませんでした");
                    }

                    CheckImportedAssets(outputAssetPath, errors);
                }
            }

            if (errors.Count > 0)
            {
                var context = AssetDatabase.LoadAssetAtPath<Object>(outputAssetPath);
                Debug.LogError($"{LogPrefix} {displayName} でエラーが出たので止めました\n- {string.Join("\n- ", errors)}\n\n書き出しのスクリプトの出力:\n{exporterOutput}", context);
                return GeneralReturnCode.Failed;
            }

            Debug.Log($"{LogPrefix} ({index + 1}/{count}) {displayName} を書き出しました\n\n書き出しのスクリプトの出力:\n{exporterOutput}");
            return GeneralReturnCode.Succeeded;
        }

        // フォルダの名前から、= から @ の前までのコメントを除く(afl の AutoAddressing と同じく最後の = から)
        // 例: Enemy_5020=ダイエイユー@bundle → Enemy_5020@bundle
        // MEMO: 書き出しのスクリプト(spine_actor_export_to_unity.py の remove_folder_comment)と同じ決まり
        static string RemoveFolderComment(string name)
        {
            int atIndex = name.LastIndexOf('@');
            string head = atIndex >= 0 ? name.Substring(0, atIndex) : name;
            string tail = atIndex >= 0 ? name.Substring(atIndex) : "";
            int equalIndex = head.LastIndexOf('=');
            if (equalIndex >= 0) head = head.Substring(0, equalIndex);
            return head + tail;
        }

        // コメントを除いた名前が同じで、名前が違うフォルダが書き出し先にあれば、元データのフォルダの名前に変える
        // 元データのフォルダにコメントを足した・変えた・消したときも、同じアセットを上書きするため(GUID を変えない)
        static void RenameOutputFolder(string destinationDirectory, string categoryName, string folderName, List<string> errors)
        {
            string categoryPath = $"{destinationDirectory}/{categoryName}";
            if (AssetDatabase.IsValidFolder(categoryPath) == false) return;

            string outputAssetPath = $"{categoryPath}/{folderName}";
            string key = RemoveFolderComment(folderName);
            var others = AssetDatabase.GetSubFolders(categoryPath)
                .Where(x => Path.GetFileName(x) != folderName && RemoveFolderComment(Path.GetFileName(x)) == key)
                .OrderBy(x => x, System.StringComparer.Ordinal)
                .ToList();
            if (others.Count == 0) return;

            if (AssetDatabase.IsValidFolder(outputAssetPath) == true || others.Count > 1)
            {
                var names = others.Select(Path.GetFileName).ToList();
                if (AssetDatabase.IsValidFolder(outputAssetPath) == true) names.Add(folderName);
                errors.Add($"コメント(= から @ の前まで)を除いた名前が同じフォルダが、書き出し先に複数あります。使うものを 1 つ残して、ほかを消してください: {categoryPath}({string.Join(", ", names)})");
                return;
            }

            string oldPath = others[0];
            string error = AssetDatabase.MoveAsset(oldPath, outputAssetPath);
            if (string.IsNullOrEmpty(error) == false)
            {
                errors.Add($"書き出し先のフォルダの名前を変えられませんでした: {oldPath} → {outputAssetPath}: {error}");
                return;
            }
            Debug.Log($"{LogPrefix} 書き出し先のフォルダの名前を変えました: {oldPath} → {outputAssetPath}");
        }

        // 書き出しのスクリプトを実行し、終了コードを返す(起動できなければ -1)
        static async UniTask<int> RunExporterAsync(string inputFolder, string exportDirectory, StringBuilder output, string progressTitle, string displayName, float progress)
        {
            string toolsDirectory = EUtility.GetRootPath() + "tools";
            string scriptPath = Path.GetFullPath(Path.Combine(toolsDirectory, ExporterScriptPath));
            if (File.Exists(scriptPath) == false)
            {
                output.AppendLine($"書き出しのスクリプトがありません: {scriptPath}");
                return -1;
            }

            // MEMO: Spine の場所は、スクリプトが環境変数 SPINE_PATH・Unity プロジェクトの .env・標準のインストール先から探す
            // MEMO: --no_dialog で、スクリプトのダイアログ(進み具合・終了の知らせ)を出さない(出すと OK を押すまで次へ進まない)
            var startInfo = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = PythonCommand,
                Arguments = $"\"{scriptPath}\" --inputs \"{inputFolder}\" --export_dir \"{exportDirectory}\" --no_dialog",
                WorkingDirectory = toolsDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            // MEMO: Windows のコンソールの文字コード(cp932)のままだと、日本語の出力が化ける
            startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            // スクリプトがかけるロック(ExternalEditLock)で、プログレスバーを出して操作を止めない(こちらのプログレスバーを出すため)
            startInfo.EnvironmentVariables["AFL_EXTERNAL_EDIT_LOCK_QUIET"] = "1";

            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo = startInfo;
                // MEMO: 出力は別のスレッドから届く
                process.OutputDataReceived += (_, e) => AppendLine(output, e.Data);
                process.ErrorDataReceived += (_, e) => AppendLine(output, e.Data);

                try
                {
                    process.Start();
                }
                catch (System.Exception e)
                {
                    output.AppendLine($"Python({PythonCommand})を起動できません。Python 3 を入れて PATH を通してください: {e.Message}");
                    return -1;
                }
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (process.HasExited == false)
                {
                    // 書き出しの間だけキャンセルを受け付ける(今の件が終わってから止める)
                    if (EditorUtility.DisplayCancelableProgressBar(progressTitle, $"書き出し中: {displayName}", progress) == true)
                    {
                        isCancelRequested = true;
                    }
                    await EUtility.WaitForEditor(100);
                }
                // MEMO: 非同期で読んでいる出力を最後まで受け取る
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        static string PythonCommand => Application.platform == RuntimePlatform.WindowsEditor ? "python" : "python3";

        static void AppendLine(StringBuilder output, string? line)
        {
            if (line == null) return;
            lock (output)
            {
                output.AppendLine(line);
            }
        }

        // 取り込みが終わるまで待つ。時間切れなら Failed
        static async UniTask<int> WaitForImportAsync()
        {
            double startTime = EditorApplication.timeSinceStartup;
            double idleStartTime = -1.0;
            while (true)
            {
                double now = EditorApplication.timeSinceStartup;
                bool isBusy = EditorApplication.isCompiling
                    || EditorApplication.isUpdating
                    || SpineAssetPostprocessor.IsProcessing;
                if (isBusy == true)
                {
                    idleStartTime = -1.0;
                }
                else if (idleStartTime < 0.0)
                {
                    idleStartTime = now;
                }
                else if (now - idleStartTime >= ImportIdleSeconds)
                {
                    return GeneralReturnCode.Succeeded;
                }

                if (now - startTime >= ImportTimeoutSeconds) return GeneralReturnCode.Failed;

                await EUtility.WaitForEditor(100);
            }
        }

        // 書き出し先の SkeletonDataAsset が、エラーなく取り込まれたか確かめる
        static void CheckImportedAssets(string outputAssetPath, List<string> errors)
        {
            if (AssetDatabase.IsValidFolder(outputAssetPath) == false)
            {
                errors.Add($"書き出し先のフォルダがありません: {outputAssetPath}");
                return;
            }

            var guids = AssetDatabase.FindAssets($"t:{nameof(SkeletonDataAsset)}", new[] { outputAssetPath });
            if (guids.Length == 0)
            {
                errors.Add($"SkeletonDataAsset が作られていません: {outputAssetPath}");
                return;
            }

            foreach (var guid in guids)
            {
                string skeletonDataAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                string extraDataPath = SpineUtilityEditor.GetSpineExtraDataPath(skeletonDataAssetPath);
                var extraData = AssetDatabase.LoadAssetAtPath<SpineExtraDataScriptableObject>(extraDataPath);
                if (extraData == null)
                {
                    errors.Add($"SpineExtraData が作られていません: {extraDataPath}");
                }
                else if (extraData.IsImportError == true)
                {
                    // MEMO: スケルトンの名前を変えたときに残った古い SkeletonDataAsset でも出る
                    errors.Add($"取り込みでエラーになりました(SpineExtraData の IsImportError。古い SkeletonDataAsset が残っているなら消してください): {skeletonDataAssetPath}");
                }
            }
        }

        // 生きている間に出たエラーログ(Error・Exception・Assert)を集める
        class ErrorLogWatcher : System.IDisposable
        {
            readonly List<string> errors;

            public ErrorLogWatcher(List<string> errors)
            {
                this.errors = errors;
                Application.logMessageReceived += OnLogMessageReceived;
            }

            public void Dispose()
            {
                Application.logMessageReceived -= OnLogMessageReceived;
            }

            void OnLogMessageReceived(string condition, string stackTrace, LogType type)
            {
                if (type == LogType.Log || type == LogType.Warning) return;
                errors.Add($"{type}: {condition}");
            }
        }
    }
}
#nullable restore
