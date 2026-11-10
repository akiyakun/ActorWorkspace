#nullable enable
using ActorWorkspace.ArcaneLedger.ActorBehaviour;

namespace ActorWorkspace.ArcaneLedger
{
    // FIXME: 適当な名前
    // ゲームごとの計算・判定(ArcaneLedgerBehaviour が持つ。既定の実装は DefaultALProcessor)
    // MEMO: 基本的にはActorイベントなどよりも先にProcessorのコールバックメソッドが呼ばれる設計
    public interface IALProcessor
    {
        // 設定(ステータスの表の大きさ・最大 HP と移動速度のステータスの ID)
        // MEMO: Setup() より前に読む(ArcaneLedgerBehaviour が表を作るときに使う)
        public ALProcessorConfig Config { get; }

        public void Setup(ArcaneLedgerBehaviour owner, StatusEffectController statusEffectController);
        public void Restore();

        public void Prepare();
        public void Update(float deltaTime);
        public void FixedUpdate(float deltaTime);

        // 汎用パラメータ(台帳の ArcaneLedgerBehaviour.GeneralParam と同じもの。計算は ALGeneralParam.Calc())
        // MEMO: 当たり判定(IALCollisionExtraData)から、攻撃した側の汎用パラメータを読むときの入口(ALHitResolver が当たった瞬間の値を写す)
        public ALGeneralParam GeneralParam { get; }

        public void OnStatusEffectChanged(uint enableFlags);

        // 当たり判定の当たり(ALHurtBoxBehaviour が陣営・ヒット履歴・部位を確かめたあと)
        public HitResult Hit(in ALHitContact contact);

        // ダメージを受ける(ALActorEvents.RequestDamage からも呼ばれる)
        public ALDamageResult ApplyDamage(ALDamageInfo info);
        // HP を回復する(ALActorEvents.RequestHeal からも呼ばれる)
        public ALHealResult ApplyHeal(float amount);
    }
}
#nullable restore
