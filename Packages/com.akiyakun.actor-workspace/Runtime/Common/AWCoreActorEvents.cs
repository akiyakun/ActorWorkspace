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


        #region ArcaneLedger
        public static readonly (string, int) DamageReaction = ("DamageReaction", 0);
        public static readonly (string, uint) OnStatusEffectChanged = ("OnStatusEffectChanged", 0);
        #endregion


        public static readonly (string, CollisionContactInfo) OnInteractHurtbox1 = ("OnInteractHurtbox1", default);
    }
}
#nullable restore