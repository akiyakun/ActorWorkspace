#nullable enable
using System.Runtime.CompilerServices;
using afl;

namespace ActorWorkspace.ArcaneLedger
{
    public abstract class StatusEffect
    {
        public event System.Action<int, bool>? OnEnableChanged;

        public abstract int Id { get; }

        bool _enable;
        public bool Enable { get => _enable; protected set => SetEnable(value);}

        // public uint StackableMask { get; protected set; }

        public float ElapsedTime { get; protected set; }
        public float RemainingTime { get; protected set; }

        protected StatusEffectController controller = null!;
        protected IAWActor Actor = null!;

        // Tick間隔時間(ダメージ発生間隔)
        float tickIntervalTime;
        float tickCountdown;

        protected UnionPrimitiveData[] Params = new UnionPrimitiveData[StatusEffectController.MaxGeneralParamCount];

        public void Setup(StatusEffectController controller)
        {
            if (this.controller != null) throw new System.InvalidOperationException("Already setup");
            this.controller = controller;

            // Id = id;
            Actor = controller.Actor;

            for (int i = 0; i < Params.Length; i++)
            {
                Params[i] = new UnionPrimitiveData();
            }

            Awake();
        }

        public virtual void Restore()
        {
            Enable = false;

            ElapsedTime = 0.0f;
            RemainingTime = -1.0f;
            tickIntervalTime = 0.0f;
            tickCountdown = 0.0f;

            // OnRestore();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetEnable(bool enable)
        {
            if (enable == true && _enable == false)
            {
                _enable = true;
                OnEnable();
                OnEnableChanged?.Invoke(Id, _enable);
            }
            else if (enable == false && _enable == true)
            {
                _enable = false;
                OnDisable();
                OnEnableChanged?.Invoke(Id, _enable);
            }
        }

        public virtual bool Apply(ApplyStatusEffectParams applyParam)
        {
            // if (OnRequest(param) == false) return;

            Enable = true;
            ElapsedTime = 0.0f;
            RemainingTime = applyParam.DurationTime;
            tickIntervalTime = applyParam.TickIntervalTime;
            tickCountdown = tickIntervalTime;

            return true;
        }

        /// <summary>
        /// EnableがfalseのときでもPrepare()は呼ばれる仕様です
        /// </summary>
        public virtual void Prepare()
        {
        }

        public void Update(float deltaTime)
        {
            if (Enable == false) return;

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
                    Enable = false;
                }
            }
        }

        protected virtual void Awake() { }
        // protected virtual void OnRestore() { }
        // protected virtual bool OnRequest(ApplyStatusEffectParams param) { return true; }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        // protected virtual void OnPrepare() { }
        protected virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnTick() { }
    }
}
#nullable restore