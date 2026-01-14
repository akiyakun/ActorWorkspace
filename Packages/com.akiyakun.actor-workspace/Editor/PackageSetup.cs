using System.IO;
using UnityEditor;
using afl;
using afl.Editor;

namespace ActorWorkspace.Editor
{
    public class PackageSetup
    {
        // See also: Packages/cyou.sumomo.afl/Editor/Custom/AppMenu.cs
        [MenuItem(AppMenu.Develop_afl + "ActorWorkspace Package Setup", false, MenuItems.Priority.Develop + 10002)]
        static void MenuPackageSetup() => DoPackageSetup();

        [InitializeOnLoadMethod]
        static void InitializeOnLoadSetup()
        {
            // 起動時一回のみ処理する
            // MEMO: ファイル読み込み確認する時はここをコメントアウトしないとreturnされます
            if (EUtility.IsInitializeOnLoadTiming == false) return;

            DoPackageSetup();
        }

        public static void DoPackageSetup()
        {
            // External~ のファイルのコピー
            var externalPath = Path.GetFullPath(ActorWorkspace.Environment.PackageRootPath) + "External~/";

            // spine
            {
                string src = Utility.PathCombine(externalPath, "tools/spine");
                string dest = Utility.PathCombine(EUtility.GetRootPath(), "tools/spine");
                Utility.CopyDirectory(src, dest, true, checkTimeStamp: false);
            }

            // bin
            {
                string src = Utility.PathCombine(externalPath, "tools/bin");
                string dest = Utility.PathCombine(EUtility.GetRootPath(), "tools/bin");
                Utility.CopyDirectory(src, dest, overwrite: true, checkTimeStamp: false);
            }
        }
    }
}