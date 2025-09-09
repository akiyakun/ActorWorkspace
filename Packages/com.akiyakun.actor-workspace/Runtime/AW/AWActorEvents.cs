#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // 本来static classだが継承して使いたいのでinterfaceになっています
    public interface AWActorEvents
    {
        #region Behaviour
        public static readonly string OnMoveBegin = "OnMoveBegin";
        public static readonly string OnMove = "OnMove";
        public static readonly string OnMoveEnd = "OnMoveEnd";
        #endregion

        #region Motion
        // public static readonly string OpenMainMenu = "OpenMainMenu";
        // public static readonly string CloseMainMenu = "CloseMainMenu";
        #endregion
    }
}
#nullable restore