#nullable enable
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    // 命令を送るものは動詞+
    // 受信するものはOn+
    //
    // 本来static classだが継承して使いたいのでinterfaceになっています
    public interface AWCoreActorEvents
    {
        #region Controls
        // public static readonly (string, bool) Exclusive = ("Exclusive", default);
        public static readonly (string, bool) SetEnableInput = ("SetEnableInput", default);
        #endregion

        #region Behaviour
        public static readonly (string, bool) OnGround = ("OnGround", default);
        #endregion

        public static readonly (string, bool) SetDisplayVisibility = ("SetDisplayVisibility", default);


        public static readonly (string, CollisionDetectorInfo) OnInteractHurtbox1 = ("OnInteractHurtbox1", default);
    }
}
#nullable restore