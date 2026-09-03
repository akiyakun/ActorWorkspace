#nullable enable
using UnityEngine;
using afl;
using afl.Service.Effects;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // EffectModifier版のIALCollisionExtraData実装
    //
    // See also: ALCollisionExtraData.cs
    [DisallowMultipleComponent]
    public class EMCollisionExtraData : EffectModifierComponent, IALCollisionExtraData
    {
        [Header("EffectModifier版のIALCollisionExtraData実装")]
        [Space(10)]
        [SerializeField] int intValue;

        [SerializeField] uint flags;

        protected override bool EnableValidateInPrefabMode { get; } = false;
        public override UpdateFlags UpdateFlags => UpdateFlags.Manual;


        #region IALCollisionExtraData
        public IAWActor? Actor { get; protected set; }
        public IALProcessor? ALProcessor { get; protected set; }

        public uint SerialNumber { get; protected set; }
        public uint Flags => flags;

        // [SerializeField] int intValue;
        public virtual int IntValue { get => intValue; protected set => intValue = value; }

        [SerializeField] float floatValue;
        public virtual float FloatValue { get => floatValue; protected set => floatValue = value; }

        [SerializeField] string stringValue = string.Empty;
        public virtual string StringValue { get => stringValue; protected set => stringValue = value; }

        public virtual object? UserData { get; protected set; }
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
                var alb = Actor.ActorBehaviourController.Get<ArcaneLedgerBehaviour>();
                if (alb == null) throw new System.Exception("ActorBehaviourControllerにArcaneLedgerBehaviourが存在しません。");

                ALProcessor = alb.Processor;
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