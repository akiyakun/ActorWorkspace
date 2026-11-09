#nullable enable

namespace ActorWorkspace
{
    // 当たり判定(HurtBox・HitBox)の追加情報
    // アニメーションの追加情報(SpineExtraDataScriptableObject など)が IAWHitAreaInfoProvider として持ち、ArcaneLedger が読む
    // MEMO: Spine 連携と ArcaneLedger の両方から使うので、中核に置く(UnitySpine から ArcaneLedger を参照しないため)

    // やられ判定(HurtBox)の部位
    [System.Serializable]
    public struct AWHurtBoxInfo
    {
        // HurtBox のアタッチメントの名前(ボーン・スロットの名前)
        public string Name;
        // 部位(0 はなし。意味はゲームが決める。例: 1 胴・2 頭・3 弱点)
        public int Region;
        // 部位のダメージ倍率(0 以下は 1 とみなす)
        public float DamageMultiplier;
        // 同じ攻撃が同じフレームに複数の HurtBox に当たったとき、大きいものを使う
        public int Priority;
    }

    // アニメーションイベントの攻撃の番号と、攻撃の情報の対応
    [System.Serializable]
    public struct AWAttackBinding
    {
        // アニメーションイベント(AWCoreAnimationEvents.SetAttack)の int
        public int LocalId;
        // ゲームが決める攻撃の ID(マスタの Id・スキルの Id など)
        public int AttackId;
        // この攻撃を持たせる HitBox のアタッチメントの名前。空ならすべての HitBox
        public string[] HitBoxNames;
        // 同じ相手に当たり直すまでの秒数(0 なら HitBox が出ている間に 1 回だけ)
        public float RepeatHitInterval;
    }

    public interface IAWHitAreaInfoProvider
    {
        bool TryGetHurtBoxInfo(string hurtBoxName, out AWHurtBoxInfo info);
        bool TryGetAttackBinding(int localId, out AWAttackBinding binding);
    }
}
#nullable restore
