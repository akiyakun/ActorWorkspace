#nullable enable
using UnityEngine;
using afl;
using ActorWorkspace.ArcaneLedger;

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
        #endregion// Controls

        #region ActorBehaviour
        public static readonly string Spawn = "Spawn";
        public static readonly string Despawn = "Despawn";

        public static readonly (string, bool) OnGround = ("OnGround", default);
        public static readonly string OnDead = "OnDead";

        public static readonly (string, int) OnFacingDirection2DChanged = ("OnFacingDirection2DChanged", default);
        #endregion// ActorBehaviour

        public static readonly (string, bool) SetDisplayVisibility = ("SetDisplayVisibility", default);


        #region AnimationEvent
        // モーションのAudioイベント(値:SEマスタのId)。AWAudioEventBehaviour が鳴らす
        public static readonly (string, int) Audio = ("Audio", default);
        // 「モーション中断で止める」SEを止める(AWAudioEventBehaviour)
        // MEMO: アクターのマネージャーの Despawn() は Behaviour に知らせないので、直接 Despawn するときは先にこれを発行する
        public static readonly string StopMotionSE = "StopMotionSE";

        public struct EffectInfo
        {
            public string Name;
            public int Id;
        }
        public static readonly (string, EffectInfo) Effect = ("Effect", default);
        #endregion// AnimationEvent


        #region ArcaneLedger
        public static readonly (string, int) DamageReaction = ("DamageReaction", 0);

        public static readonly (string, int id) RevokeStatusEffect = ("RevokeStatusEffect", 0);
        public static readonly (string, ApplyStatusEffectParams) RequestStatusEffect = ("RequestStatusEffect", default);
        public static readonly (string, uint enableFlags) OnStatusEffectChanged = ("OnStatusEffectChanged", 0);
        #endregion// ArcaneLedger


        public static readonly (string, CollisionContactInfo) OnInteractHurtbox1 = ("OnInteractHurtbox1", default);
    }
}
#nullable restore