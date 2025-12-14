using UnityEngine;

namespace ActorWorkspace
{
    // RuntimeでもEditorでも使うリテラル等を定義したりする用
    public static class Environment
    {
#if UNITY_EDITOR
        public const string PackageRootPath = "Packages/com.akiyakun.actor-workspace/";
#endif
    }
}
