#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // 命令を送るものは動詞+
    // 受信するものはOn+
    //
    // 本来static classだが継承して使いたいのでinterfaceになっています
    public interface AWActorEvents
    {
        #region Behaviour
        public static readonly (string, bool) OnGround = ("OnGround", default);

        public static readonly string OnMoveBegin = "OnMoveBegin";
        public static readonly string OnMove = "OnMove";
        public static readonly string OnMoveEnd = "OnMoveEnd";

        public static readonly string OnJump = "OnJump";
        public static readonly string OnFastFall = "OnFastFall";
        #endregion

        #region Motion
        // public static readonly string OpenMainMenu = "OpenMainMenu";
        // public static readonly string CloseMainMenu = "CloseMainMenu";
        #endregion
    }
}
#nullable restore