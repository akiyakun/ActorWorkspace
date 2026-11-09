#nullable enable
using System.Runtime.CompilerServices;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;
using afl;

namespace ActorWorkspace.ArcaneLedger
{
    // FIXME: 適当な名前
    // MEMO: 基本的にはActorイベントなどよりも先にProcessorのコールバックメソッドが呼ばれる設計
    public interface IALProcessor
    {
        // public IAWActor Actor { get; }

        public void Setup(ArcaneLedgerBehaviour owner, StatusEffectController statusEffectController);
        public void Restore();

        public void Prepare();
        public void Update(float deltaTime);
        public void FixedUpdate(float deltaTime);

        // public ALGeneralParam GetGeneralParam(int paramId);

        public void OnStatusEffectChanged(uint enableFlags);

        public HitResult Hit(CollisionContactInfo contactInfo);

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Calc(int paramId);

        // public void SetupStatusEffects(IStatusEffect[] statusEffects);

    }
}
#nullable restore