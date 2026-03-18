#nullable enable
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    // RootMotion情報
    public struct AWRootMotionInfo
    {
        // 初期状態でRootMotionを使用する設定にするかどうか
        public bool DefaultUseRootMotion;

        // 初期状態でRootMotion適用状態にするかどうか
        public bool DefaultApplyRootMotion;

        public bool DefaultApplyRootMotionPositionX;
        public bool DefaultApplyRootMotionPositionY;
    }
}
#nullable restore