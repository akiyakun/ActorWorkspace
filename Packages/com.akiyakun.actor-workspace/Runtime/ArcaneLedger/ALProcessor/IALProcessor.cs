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

        // ステータスの表(ALStatTable)の大きさ。0 ならステータスを持たない
        // MEMO: Setup() より前に読む(ArcaneLedgerBehaviour が表を作るときに使う)
        public int StatCapacity { get; }
        // 最大 HP のステータスの ID。-1 なら HP を持たない
        public int MaxLifeStatId { get; }
        // 移動速度のステータスの ID(減速・ノックバックの効果が補正を付ける)。-1 なら付けない
        public int MoveSpeedStatId { get; }

        public void Setup(ArcaneLedgerBehaviour owner, StatusEffectController statusEffectController);
        public void Restore();

        public void Prepare();
        public void Update(float deltaTime);
        public void FixedUpdate(float deltaTime);

        // public ALGeneralParam GetGeneralParam(int paramId);

        public void OnStatusEffectChanged(uint enableFlags);

        // 当たり判定の当たり(ALHurtBoxBehaviour が陣営・ヒット履歴・部位を確かめたあと)
        public HitResult Hit(in ALHitContact contact);

        // ダメージを受ける(ALActorEvents.RequestDamage からも呼ばれる)
        public ALDamageResult ApplyDamage(ALDamageInfo info);
        // HP を回復する(ALActorEvents.RequestHeal からも呼ばれる)
        public ALHealResult ApplyHeal(float amount);

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Calc(int paramId);

        // public void SetupStatusEffects(IStatusEffect[] statusEffects);

    }
}
#nullable restore