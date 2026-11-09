#nullable enable

namespace ActorWorkspace.ArcaneLedger
{
    // ステータスの補正(Modifier)の演算
    // 最終値 = (基礎値 + ΣAdd) × (1 + ΣPercentAdd) × ΠMultiply × min(MultiplyMin)
    // Override があれば、最後に足した Override の値にする(ほかの補正は使わない)
    // MEMO: ゲーム側で数値として保存することがあるので、末尾に追加すること
    public enum ALModifierOp
    {
        Add = 0,            // 足す
        PercentAdd = 1,     // 割合を足す(0.1 で +10%。ほかの PercentAdd と足し合わせてから掛ける)
        Multiply = 2,       // 掛ける(ほかの Multiply とも掛け合わせる)
        MultiplyMin = 3,    // MultiplyMin の中でいちばん小さいものだけを掛ける(減速が重なったら、いちばん遅くなるものを使う など)
        Override = 4,       // この値にする
    }
}
#nullable restore
