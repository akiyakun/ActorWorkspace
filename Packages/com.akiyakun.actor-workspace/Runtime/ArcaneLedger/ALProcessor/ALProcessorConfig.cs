#nullable enable
namespace ActorWorkspace.ArcaneLedger
{
    // ALProcessor の設定(IALProcessor.Config)
    // MEMO: ArcaneLedgerBehaviour が Setup() より前に読む(ステータスの表・HP を作るため)。なので、ALProcessor の状態によらない値にする
    public struct ALProcessorConfig
    {
        // ステータスの表(ALStatTable)の大きさ。0 ならステータスを持たない
        public int StatCapacity;
        // 最大 HP のステータスの ID。-1 なら HP を持たない
        public int MaxLifeStatId;
        // 移動速度のステータスの ID(減速・ノックバックの効果が補正を付ける)。-1 なら付けない
        public int MoveSpeedStatId;

        // ステータス・HP を持たない
        public static ALProcessorConfig Default => new() { StatCapacity = 0, MaxLifeStatId = -1, MoveSpeedStatId = -1 };
    }
}
#nullable restore
