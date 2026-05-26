#nullable enable
namespace ActorWorkspace.ArcaneLedger
{
    public interface IStatusEffect
    {
        public int Id { get; }

        // 有効になってからの経過時間
        public float ElapsedTime { get; protected set; }
        // 残り効果時間
        public float RemainingTime { get; protected set; }

        public void Setup(StatusEffectController owner);
        public void Restore();

        public void Request(ApplyStatusEffectParams param);

        public void DoEnable();
        public void DoDisable();
        public void DoPrepare();
        public void DoUpdate(float deltaTime);

    }
}
#nullable restore