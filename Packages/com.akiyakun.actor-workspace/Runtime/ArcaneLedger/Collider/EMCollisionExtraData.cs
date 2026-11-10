#nullable enable
using UnityEngine;
using afl;
using afl.Service.Effects;

namespace ActorWorkspace.ArcaneLedger
{
    // EffectModifier版のIALCollisionExtraData実装(パーティクルの弾など、エフェクトの当たり判定)
    // 撃ったアクターは EffectOption.UserData で渡す
    //
    // See also: ALCollisionExtraData.cs
    [DisallowMultipleComponent]
    public class EMCollisionExtraData : EffectModifierComponent, IALCollisionExtraData
    {
        [Header("EffectModifier版のIALCollisionExtraData実装")]
        [Space(10)]
        [SerializeField] int intValue;

        [SerializeField] uint flags;

        [SerializeField] int attackId;

        // 同じ相手に何度も当たるのを防ぐ方法(既定は None: 当たるたびに処理する)
        // MEMO: 1 回の再生(Effect.Play)で出た粒は同じ SerialNumber になり、次の再生で飛んでいる粒の番号も変わるので、PerActivation は向かない
        //       当たっても消えない粒(貫通・置く・範囲)には PerSource と RepeatHitInterval を使う
        [SerializeField] ALHitHistoryMode hitHistoryMode = ALHitHistoryMode.None;
        [SerializeField] float repeatHitInterval;

        protected override bool EnableValidateInPrefabMode { get; } = false;
        public override UpdateFlags UpdateFlags => UpdateFlags.Manual;


        #region IALCollisionExtraData
        public IAWActor? Actor { get; protected set; }
        public IALProcessor? ALProcessor { get; protected set; }

        // 再生した回(Effect.Play)の番号。同じ回に出た粒はすべて同じ番号
        public uint SerialNumber { get; protected set; }
        public uint Flags => flags;

        // [SerializeField] int intValue;
        public virtual int IntValue { get => intValue; protected set => intValue = value; }

        [SerializeField] float floatValue;
        public virtual float FloatValue { get => floatValue; protected set => floatValue = value; }

        [SerializeField] string stringValue = string.Empty;
        public virtual string StringValue { get => stringValue; protected set => stringValue = value; }

        public virtual object? UserData { get; protected set; }

        // 撃ったアクターの陣営(再生したときに写す。撃ったアクターが消えても残る)
        public int Faction { get; protected set; }
        public virtual int AttackId => attackId;
        public ALHitHistoryMode HitHistoryMode => hitHistoryMode;
        public float RepeatHitInterval => repeatHitInterval;

        // MEMO: エフェクトは HurtBox にならないので、部位は使わない
        public int Region => 0;
        public float DamageMultiplier => 1.0f;
        public int Priority => 0;
        #endregion


        protected override bool OnSetup()
        {
            return true;
        }

        protected override void OnRestore()
        {
            Actor = null;
            ALProcessor = null;
            SerialNumber = 0;
            Faction = 0;
            // IntValue = 0;
            // FloatValue = 0.0f;
            // StringValue = string.Empty;
            UserData = null;
        }

        public override void OnPrepareToPlay()
        {
            Actor = Effect.Option.UserData as IAWActor;
            if (Actor != null)
            {
                var alb = Actor.GetLedger();
                if (alb == null) throw new System.Exception("ActorBehaviourControllerにArcaneLedgerBehaviourが存在しません。");

                ALProcessor = alb.Processor;
                Faction = alb.Faction;
                SerialNumber = ColliderSerialNumberGenerator.GetNext();
            }
        }

        protected override void Evaluate(float deltaTime)
        {
        }


#if UNITY_EDITOR
        // [Space(10)]
        [Header("Debug")]
        [SerializeField] bool debugBreakPause = false;
        public bool DebugBreakPause { get => debugBreakPause; set => debugBreakPause = value; }
#endif
    }
}
#nullable restore
