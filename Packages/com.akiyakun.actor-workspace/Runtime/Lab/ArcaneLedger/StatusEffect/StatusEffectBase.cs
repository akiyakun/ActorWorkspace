#nullable enable
using afl;

namespace ActorWorkspace.ArcaneLedger
{
    public abstract class StatusEffectBase
    {
        public abstract int Id { get; }
        public int Enable { get; protected set; }

        // public uint StackableMask { get; protected set; }

        public float ElapsedTime { get; protected set; }
        public float RemainingTime { get; protected set; }

        protected StatusEffectController owner = null!;

        // Tick間隔時間(ダメージ発生間隔)
        float tickIntervalTime;
        float tickCountdown;

        protected UnionPrimitiveData[] GeneralParams = new UnionPrimitiveData[StatusEffectController.MaxGeneralParamCount];

        public void Setup(StatusEffectController owner)
        {
            if (this.owner != null) throw new System.InvalidOperationException("Already setup");
            this.owner = owner;

            // Id = id;

            for (int i = 0; i < GeneralParams.Length; i++)
            {
                GeneralParams[i] = new UnionPrimitiveData();
            }

            OnAwake();
        }

        public virtual void Restore()
        {
            Enable = 0;

            ElapsedTime = 0.0f;
            RemainingTime = -1.0f;
            tickIntervalTime = 0.0f;
            tickCountdown = 0.0f;

            OnRestore();
        }

        public virtual void Request(ApplyStatusEffectParams param)
        {
            Enable = 1;
            ElapsedTime = 0.0f;
            RemainingTime = param.DurationTime;
            tickIntervalTime = param.TickIntervalTime;
            tickCountdown = tickIntervalTime;
        }

        public void DoEnable()
        {
            OnEnable();
        }

        public void DoDisable()
        {
            OnDisable();
        }

        public void DoPrepare()
        {
            // if (Enable <= 0) return;
            // 無効状態でもPrepareは呼ぶ
            OnPrepare();
        }

        public void DoUpdate(float deltaTime)
        {
            if (Enable <= 0) return;

            OnUpdate(deltaTime);

            ElapsedTime += deltaTime;

            // Tick間隔時間が有効なとき
            if (tickIntervalTime > 0.0f)
            {
                tickCountdown -= deltaTime;
                if (tickCountdown <= 0.0f)
                {
                    OnTick();
                    tickCountdown = tickIntervalTime;
                }
            }

            // 効果時間が有効なとき
            if (RemainingTime >= 0.0f)
            {
                RemainingTime -= deltaTime;
                if (RemainingTime < 0.0f)
                {
                    DoDisable();
                }
            }
        }

        protected virtual void OnRestore() { }
        protected virtual void OnAwake() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnPrepare() { }
        protected virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnTick() { }
    }
}
#nullable restore