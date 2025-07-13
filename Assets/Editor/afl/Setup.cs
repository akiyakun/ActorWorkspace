using System.IO;
using UnityEngine;
using UnityEditor;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace afl.Setup
{
	public static class Setup
	{
		// MEMO:
		// [InitializeOnLoadMethod]でsymlinkを作成を試みたときの残骸
#if false
		[System.Serializable]
		public class AppConfig
		{
			[System.Serializable]
			public class EditorSupport
			{
				public bool create_symlink_for_libs_packages;
				public int create_symlink_date;
			}
			public EditorSupport editor_support;
		}


		[MenuItem("App/Test", false, 0)]
		static void Test()
		{
			// OnSetup();

			CallOSScript("setup", workDir: "tools/utility/", onFinish: (exitCode) =>
			{
				Debug.Log($"CallOSScript finished with exit code: {exitCode}");
				if (exitCode == 0)
				{
					Debug.Log("Setup completed successfully.");

					// パッケージを解決する
					// https://docs.unity3d.com/ja/2022.3/ScriptReference/PackageManager.Client.Resolve.html
					UnityEditor.PackageManager.Client.Resolve();
				}
			});

		}

		// [InitializeOnLoadMethod]
		static void OnSetup()
		{
			// if (EditorApplication.timeSinceStartup > 30.0) return;

			string str = File.ReadAllText($"{Directory.GetCurrentDirectory()}/app_config.json");
			var appConfig = JsonUtility.FromJson<AppConfig>(str);

			Debug.Log("AppConfig: " + appConfig.editor_support.create_symlink_date);

			int i = 0;
			try
			{
				// i = int.Parse(EditorUserSettings.GetConfigValue($"afl_create_symlink_date"));
				i = 0;
			}
			catch
			{
				i = 0;
			}
			Debug.Log($"i: {i}");
			// i++;
			// i = 0;

			if (appConfig.editor_support.create_symlink_date > i)
			{
				EditorUserSettings.SetConfigValue($"afl_create_symlink_date", appConfig.editor_support.create_symlink_date.ToString());
				Debug.Log($"set afl_create_symlink_date");
			}
		}





		/// <summary>
		/// プロジェクトルートパスを取得
		/// 末尾に"/"が付きます。
		/// </summary>
		public static string GetRootPath()
		{
			return Path.GetDirectoryName(Application.dataPath) + "/";
		}

		#region Execute

		// https://qiita.com/skitoy4321/items/10c47eea93e5c6145d48
		static async UniTask StartProcessAsync(
			// static void StartProcessAsync(
			System.Diagnostics.ProcessStartInfo startInfo,
			System.Action<int> onFinish
		)
		{
			// await UniTask.SwitchToMainThread();
			// await UniTask.WaitForSeconds(0.1f);

			if (string.IsNullOrEmpty(startInfo.WorkingDirectory))
			{
				startInfo.WorkingDirectory = GetRootPath();
				Debug.Log($"WorkingDirectory: {startInfo.WorkingDirectory}");
			}

			// ウィンドウ表示を完全に消したい場合
			// startInfo.CreateNoWindow = true;
			startInfo.RedirectStandardError = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.UseShellExecute = false;

			int exitCode = -1;
			var ctoken = new CancellationTokenSource();

			using (var process = new System.Diagnostics.Process())
			// using (var ctoken = new CancellationTokenSource())
			{
				try
				{

					process.StartInfo = startInfo;

					// イベントを有効にする
					process.EnableRaisingEvents = true;

					// コールバックの設定
					process.OutputDataReceived += (sender, e) =>
					{
						if (e.Data != null)
						{
							Debug.Log($"stdout:{e.Data}");
						}
					};
					process.ErrorDataReceived += (sender, e) =>
					{
						if (e.Data != null)
						{
							Debug.LogError($"stderr:{e.Data}");
						}
					};
					process.Exited += (sender, e) =>
					{
						// プロセスが終了すると呼ばれる
						exitCode = process.ExitCode;

						if (process.ExitCode == 0)
						{
							Debug.Log($"ExitCode:{process.ExitCode}");
						}
						else
						{
							Debug.LogError($"ExitCode:{process.ExitCode}");
						}

						// 別スレッドなのでメインスレッドに戻す(Resources.Load等が動かない)
						// UniTask.Void(async () => {
						//     await UniTask.SwitchToMainThread();

						//     try
						//     {
						//         onFinish?.Invoke(process.ExitCode);
						//     }
						//     catch(System.Exception ex)
						//     {
						//         Debug.Log(ex.Message);
						//     }
						// });

						// await UniTask.SwitchToMainThread();

						// try
						// {
						//     onFinish?.Invoke(process.ExitCode);
						// }
						// catch(System.Exception ex)
						// {
						//     Debug.Log(ex.Message);
						// }

						Debug.Log("StartProcessAsync: ctoken.Cancel()");
						ctoken.Cancel();
						// Debug.Log($"StartProcessAsync: {ctoken.Token.IsCancellationRequested}");
					};

					// プロセスの開始
					process.Start();
					// 非同期出力読出し開始
					// MEMO: Start()の後に呼び出さないとダメらしい
					process.BeginErrorReadLine();
					process.BeginOutputReadLine();

					// Debug.Log("StartProcessAsync: wait in");

					// 終了まで待つ
					// ctoken.Token.WaitHandle.WaitOne();
					while (ctoken.Token.IsCancellationRequested == false)
					{
						// await UniTask.SwitchToMainThread();
						// Thread.Sleep(10);
						// EditorApplication.QueuePlayerLoopUpdate();
						// await UniTask.Yield(PlayerLoopTiming.Update, ctoken.Token);
						await UniTask.WaitForSeconds(1.0f, cancellationToken: ctoken.Token);
						if (ctoken.Token.IsCancellationRequested) break;
						// Debug.Log("StartProcessAsync: wait while");
					}
					Debug.Log("StartProcessAsync: wait out");

					// MEMO: Exited呼び出し後になぜかここに処理が来ない
					// finallyがあれば処理できそう

				}
				catch (System.OperationCanceledException e)
				{
					Debug.Log("Cancel:" + e.Message);
				}
				catch (System.Exception e)
				{
					Debug.LogError(e.Message);
				}
				finally
				{
					// Debug.Log("StartProcessAsync: finally");
				}
			}

			try
			{
				// Unityのメインスレッドで実行させたいためここで処理
				await UniTask.SwitchToMainThread();
				// Debug.Log("StartProcessAsync: invoke onfinish");
				await UniTask.WaitForSeconds(0.1f);
				onFinish?.Invoke(exitCode);
			}
			catch (System.Exception e)
			{
				Debug.Log(e.Message);
			}

			// Debug.Log("StartProcessAsync: DoConsoleCommandAsync(): End");
			await UniTask.WaitForSeconds(0.1f);
		}

		public static void DoConsoleCommand(string cmd, string workDir = "", System.Action<int> onFinish = null)
		{
			var startInfo = new System.Diagnostics.ProcessStartInfo();
			if (string.IsNullOrEmpty(workDir)) workDir = GetRootPath();
			startInfo.WorkingDirectory = workDir;

			startInfo.FileName = "/bin/bash";
			startInfo.Arguments = "-c \" " + cmd + " \"";

			StartProcessAsync(startInfo, onFinish).Forget();
		}

		public static void DoShellExecute(
			string fileName,
			string workDir = "",
			System.Action<int> onFinish = null,
			bool createNoWindow = false,
			string arguments = ""
		)
		{
			using (var process = new System.Diagnostics.Process())
			{
				var startInfo = process.StartInfo;
				startInfo.FileName = fileName;
				startInfo.RedirectStandardError = false;
				startInfo.RedirectStandardOutput = false;
				startInfo.UseShellExecute = true;
				startInfo.CreateNoWindow = createNoWindow;
				startInfo.Arguments = arguments;

				if (string.IsNullOrEmpty(workDir))
				{
					startInfo.WorkingDirectory = GetRootPath();
				}
				else
				{
					startInfo.WorkingDirectory = workDir;
				}

				try
				{
					process.EnableRaisingEvents = true;
					process.Exited += (object sender, System.EventArgs e) =>
					{
						onFinish?.Invoke(process.ExitCode);
						if (process.ExitCode != 0)
						{
							Debug.LogError($"ExitCode: {process.ExitCode}");
						}
					};

					process.Start();
					process.WaitForExit();
				}
				catch (System.Exception e)
				{
					Debug.LogError(e.Message);
				}
			}
		}

		//
		// fileName:
		//      拡張子なしのファイル名
		//      Windowsの場合は .bat ファイルを実行します。
		//      macOSの場合は .command ファイルを実行します。
		//
		public static void CallOSScript(string fileName, System.Action<int> onFinish = null, string workDir = "")
		{

			switch (Application.platform)
			{
				case RuntimePlatform.WindowsEditor:
					fileName += ".bat";
					Debug.Log($"CallOSScript(): {workDir}{fileName}");

					// シェル実行
					DoShellExecute(fileName: fileName, workDir: workDir, onFinish: onFinish);
					break;
				case RuntimePlatform.OSXEditor:
					fileName += ".command";
					Debug.Log($"CallOSScript(): {workDir}{fileName}");

					// macOSの場合はシェルコマンドを直接実行
					DoConsoleCommand(cmd: $"sh {fileName}", workDir: workDir, onFinish: onFinish);
					break;
				default:
					Debug.LogError("CallOSScript(): Not supported platform");
					return;
			}
		}

		#endregion
#endif
	}
}
