using System.IO;
using UnityEngine;
using UnityEditor;
using afl;
using afl.Editor;

namespace ActorWorkspace.Editor
{
    public class PackageSetup
    {
        // See also: Packages/cyou.sumomo.afl/Editor/Custom/AppMenu.cs
        public const string Develop_AW = MenuItems.Develop + "ActorWorkspace/";
        [MenuItem(Develop_AW + "ActorWorkspace Package Setup", false, MenuItems.Priority.Develop + 2/* + 10002*/)]
        static void MenuPackageSetup() => DoPackageSetup();

        [InitializeOnLoadMethod]
        static void InitializeOnLoadSetup()
        {
            // 起動時一回のみ処理する
            // MEMO: ファイル読み込み確認する時はここをコメントアウトしないとreturnされます
            if (EUtility.IsInitializeOnLoadTiming == false) return;

            DoPackageSetup();
        }

        static void DoPackageSetup()
        {
            Debug.Log("Start PackageSetup in ActorWorkspace...");

            // External~ のパスを作成
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

            Debug.Log("Completed PackageSetup in ActorWorkspace.");
        }

        [MenuItem(Develop_AW + "BehaviorDesigner Core Setup", false, MenuItems.Priority.Develop + 3/* + 10002*/)]
        static void BehaviorDesignerCoreSetup()
        {
            Debug.Log("Start BehaviorDesigner Core Setup in ActorWorkspace...");

            // External~ のパスを作成
            var externalPath = Path.GetFullPath(ActorWorkspace.Environment.PackageRootPath) + "External~/";

            // BehaviorDesigner
            {
                string src = Utility.PathCombine(externalPath, "BehaviorDesigner");
                string dest = Utility.PathCombine(EUtility.GetRootPath(), "Assets/BehaviorTree/Core");
                Utility.CopyDirectory(src, dest, overwrite: true, checkTimeStamp: false);
            }

            Debug.Log("Completed BehaviorDesigner Core Setup in ActorWorkspace.");
        }

        [MenuItem(Develop_AW + "BehaviorDesigner core files to package", false, MenuItems.Priority.Develop + 4/* + 10002*/)]
        static void BehaviorDesignerCoreFilesToPackage()
        {
            Debug.Log("Start BehaviorDesigner core files to package in ActorWorkspace...");

            // External~ のパスを作成
            var externalPath = Path.GetFullPath(ActorWorkspace.Environment.PackageRootPath) + "External~/";

            // BehaviorDesigner
            {
                string src = Utility.PathCombine(EUtility.GetRootPath(), "Assets/BehaviorTree/Core");
                string dest = Utility.PathCombine(externalPath, "BehaviorDesigner");
                Utility.CopyDirectory(src, dest, overwrite: true, checkTimeStamp: false);
            }

            Debug.Log("Completed BehaviorDesigner core files to package in ActorWorkspace.");
        }
    }
}