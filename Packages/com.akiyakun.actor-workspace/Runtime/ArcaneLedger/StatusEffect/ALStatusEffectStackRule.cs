#nullable enable

namespace ActorWorkspace.ArcaneLedger
{
    // 重ね掛けのルール(StatusEffectController.ExclusiveMask の状態異常に、別の種類がかかったとき)
    // 同じ種類をかけ直したときは、どのルールでも効果時間などが新しいものになる
    // MEMO: ゲーム側で数値として保存することがあるので、末尾に追加すること
    public enum ALStatusEffectStackRule
    {
        KeepCurrent = 0,    // 前維持。今の効果が終わるまで、ほかの種類はかからない
        Overwrite = 1,      // 上書き。今の効果(待たせているものも含む)を終わらせて、新しい効果にする
        Stack = 2,          // 重複。効果が重なる
    }
}
#nullable restore
