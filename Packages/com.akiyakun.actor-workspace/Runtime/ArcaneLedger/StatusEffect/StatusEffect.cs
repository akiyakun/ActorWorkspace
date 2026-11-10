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

        public float ElapsedTime { get; protected set; }
        public float RemainingTime { get; protected set; }

        protected StatusEffectController controller = null!;
        protected IAWActor Actor = null!;

        // アクターのステータス(補正は AddModifierWhileEnabled() で付ける)
        protected ALStatTable Stats => controller.Stats;
        // アクターの台帳(移動の向きの上書き OverrideMoveDirection など)
        protected ActorBehaviour.ArcaneLedgerBehaviour Ledger => controller.Owner;

        // Tick間隔時間(ダメージ発生間隔)
        float tickIntervalTime;
        float tickCountdown;

        // 最後に Apply() したときのパラメータ(Param1〜Param3 もここで読める)
        protected ApplyStatusEffectParams AppliedParams { get; private set; }

        // 派生クラスが自由に使う作業用の値
        // MEMO: Apply() では AppliedParams の値を写さない(派生クラスが Param1 などを変換して入れることがあるため)
        protected UnionPrimitiveData[] Params = new UnionPrimitiveData[StatusEffectController.MaxGeneralParamCount];

        // かかっている間にかけ直したとき、次の Tick までの時間を引き継ぐか
        // true: 引き継ぐ(新しい間隔より長いときだけ新しい間隔に縮める)。false: かけ直すたびに最初から数え直す
        // MEMO: 数え直すと、間隔より短い周期でかけ直しが続いたときに OnTick() が呼ばれなくなる
        protected virtual bool KeepTickOnReapply => true;

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
            AppliedParams = default;

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
                // かかっている間だけ付けたステータスの補正を外す
                RemoveModifiersWhileEnabled();
                OnEnableChanged?.Invoke(Id, _enable);
            }
        }

        // かかっている間だけステータスに補正を付ける(効果が終わると外れる)。statId が負なら何もしない
        protected void AddModifierWhileEnabled(int statId, ALModifierOp op, float value)
        {
            if (statId < 0) return;
            if (Enable == false)
            {
                UnityEngine.Debug.Assert(false, $"StatusEffect: AddModifierWhileEnabled() is called while disabled. id={Id}");
                return;
            }
            Stats.AddModifier(statId, op, value, this);
        }

        // AddModifierWhileEnabled() で付けた補正をすべて外す(かけ直して値を変えるときなど)
        protected void RemoveModifiersWhileEnabled()
        {
            Stats.RemoveModifiers(this);
        }

        public virtual bool Apply(ApplyStatusEffectParams applyParam)
        {
            bool wasEnabled = Enable;

            AppliedParams = applyParam;
            Enable = true;
            ElapsedTime = 0.0f;
            RemainingTime = applyParam.DurationTime;
            tickIntervalTime = applyParam.TickIntervalTime;
            // MEMO: 前の間隔が 0(Tick しない)だったときは、次の Tick までの時間が残っていないので、最初から数える
            if (wasEnabled == false || KeepTickOnReapply == false || tickCountdown <= 0.0f)
            {
                tickCountdown = GetFirstTickDelay(applyParam);
            }
            else if (tickCountdown > tickIntervalTime)
            {
                tickCountdown = tickIntervalTime;
            }

            return true;
        }

        // かかってから最初の Tick までの時間。ふつうは間隔と同じ
        protected virtual float GetFirstTickDelay(ApplyStatusEffectParams applyParam) => applyParam.TickIntervalTime;

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
            // MEMO: 間隔を超えた分の時間は次の Tick に持ち越す(捨てると、フレームレートが低いほど Tick が減る)。1 フレームに何回でも Tick する
            if (tickIntervalTime > 0.0f)
            {
                tickCountdown -= deltaTime;
                while (tickCountdown <= 0.0f && Enable && tickIntervalTime > 0.0f)
                {
                    OnTick();
                    tickCountdown += tickIntervalTime;
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

        // 物理の更新(ArcaneLedgerBehaviour の FixedUpdate)。かかっているときだけ呼ばれる
        // MEMO: 効果時間・Tick は Update() で数える。ここでは物理で動かす処理(Rigidbody など)だけを行う
        public void FixedUpdate(float deltaTime)
        {
            if (Enable == false) return;
            OnFixedUpdate(deltaTime);
        }

        protected virtual void Awake() { }
        // protected virtual void OnRestore() { }
        // protected virtual bool OnRequest(ApplyStatusEffectParams param) { return true; }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        // protected virtual void OnPrepare() { }
        protected virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnFixedUpdate(float deltaTime) { }
        protected virtual void OnTick() { }
    }
}
#nullable restore