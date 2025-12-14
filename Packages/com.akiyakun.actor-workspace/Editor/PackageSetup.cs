using System.IO;
using UnityEditor;
using afl;
using afl.Editor;

namespace ActorWorkspace.Editor
{
    public class PackageSetup
    {
        [InitializeOnLoadMethod]
        static void InitializeOnLoadSetup()
        {
            // 起動時一回のみ処理する
            // MEMO: ファイル読み込み確認する時はここをコメントアウトしないとreturnされます
            // if (EUtility.IsInitializeOnLoadTiming == false) return;

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
                Utility.DirectoryCopy(src, dest, true, checkTimeStamp: false);
            }

            // bin
            {
                string src = Utility.PathCombine(externalPath, "tools/bin");
                string dest = Utility.PathCombine(EUtility.GetRootPath(), "tools/bin");
                Utility.DirectoryCopy(src, dest, overwrite: true, checkTimeStamp: false);
            }
        }
    }
}