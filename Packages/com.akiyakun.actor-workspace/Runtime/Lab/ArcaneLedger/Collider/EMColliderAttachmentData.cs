#nullable enable
using UnityEngine;
using afl;
using afl.Service.Effects;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    public class EMColliderAttachmentData : EffectModifierComponent, IALColliderAttachmentData
    {
        protected override bool EnableValidateInPrefabMode { get; } = false;
        public override UpdateFlags UpdateFlags => UpdateFlags.Manual;


        #region IALColliderAttachmentData
        public IAWActor? Actor { get; protected set; }
        public IALProcessor? ALProcessor { get; protected set; }

        public uint SerialNumber { get; protected set; }

        [SerializeField] int intValue;
        public int IntValue { get => intValue; protected set => intValue = value; }

        [SerializeField] float floatValue;
        public float FloatValue { get => floatValue; protected set => floatValue = value; }

        [SerializeField] string stringValue = string.Empty;
        public string StringValue { get => stringValue; protected set => stringValue = value; }

        public object? UserData { get; protected set; }
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
            // ALProcessor = Actor.ActorBehaviourController.Get<ArcaneLedgerBehaviour>()?.Processor;
            SerialNumber = ColliderSerialNumberGenerator.GetNext();
        }

        protected override void Evaluate(float deltaTime)
        {
        }
    }
}
#nullable restore