#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.ArcaneLedger
{
    // アクターのステータス(最大 HP・攻撃力・防御力・移動速度など)の表
    // ステータスの ID は int(0〜Capacity-1)。ゲーム側の enum を (int) にして使う
    // 基礎値に補正(Modifier)を積み上げて最終値を出す(計算のしかたは ALModifierOp)
    //
    // MEMO: 攻撃モーションごとの係数(モーション値)は ALGeneralParam にある。こちらはキャラクターの能力値
    public sealed class ALStatTable
    {
        struct Modifier
        {
            public int Serial;
            public ALModifierOp Op;
            public float Value;
            public object? Source;
        }

        readonly float[] baseValues;
        readonly float[] minValues;
        readonly float[] maxValues;
        readonly float[] cachedValues;
        readonly bool[] dirtyFlags;
        readonly List<Modifier>[] modifiers;
        int lastSerial;

        // 最終値が変わったかもしれないときに知らせる(値: ステータスの ID)
        public event System.Action<int>? OnChanged;

        public int Capacity { get; }

        public ALStatTable(int capacity)
        {
            Debug.Assert(capacity >= 0, $"ALStatTable: capacity is negative. capacity={capacity}");
            Capacity = Mathf.Max(0, capacity);

            baseValues = new float[Capacity];
            minValues = new float[Capacity];
            maxValues = new float[Capacity];
            cachedValues = new float[Capacity];
            dirtyFlags = new bool[Capacity];
            modifiers = new List<Modifier>[Capacity];
            for (int id = 0; id < Capacity; id++)
            {
                minValues[id] = float.NegativeInfinity;
                maxValues[id] = float.PositiveInfinity;
                modifiers[id] = new List<Modifier>();
            }
        }

        // 基礎値
        public void SetBase(int id, float value)
        {
            CheckId(id);
            if (baseValues[id] == value) return;
            baseValues[id] = value;
            MarkDirty(id);
        }

        public float GetBase(int id)
        {
            CheckId(id);
            return baseValues[id];
        }

        // 最終値の範囲(既定は制限なし)
        public void SetRange(int id, float min, float max)
        {
            CheckId(id);
            Debug.Assert(min <= max, $"ALStatTable: min is greater than max. id={id}, min={min}, max={max}");
            minValues[id] = min;
            maxValues[id] = max;
            MarkDirty(id);
        }

        // 最終値(基礎値に補正を積み上げて、範囲に収めた値)
        public float Get(int id)
        {
            CheckId(id);
            if (dirtyFlags[id])
            {
                cachedValues[id] = Calculate(id);
                dirtyFlags[id] = false;
            }
            return cachedValues[id];
        }

        // 補正を足す。source: 出どころ(装備・スキル・状態異常など。RemoveModifiers() でまとめて外すときに使う)
        public ALModifierHandle AddModifier(int id, ALModifierOp op, float value, object? source = null)
        {
            CheckId(id);
            int serial = ++lastSerial;
            modifiers[id].Add(new Modifier() { Serial = serial, Op = op, Value = value, Source = source });
            MarkDirty(id);
            return new ALModifierHandle(this, id, serial);
        }

        // 補正を外す。外したら true
        public bool RemoveModifier(ALModifierHandle handle)
        {
            int index = FindModifier(handle);
            if (index < 0) return false;
            modifiers[handle.StatId].RemoveAt(index);
            MarkDirty(handle.StatId);
            return true;
        }

        // 出どころが同じ補正をすべて外す。戻り値: 外した数
        public int RemoveModifiers(object source)
        {
            int removed = 0;
            for (int id = 0; id < Capacity; id++)
            {
                var list = modifiers[id];
                int count = list.RemoveAll(m => ReferenceEquals(m.Source, source));
                if (count == 0) continue;
                removed += count;
                MarkDirty(id);
            }
            return removed;
        }

        public void ClearModifiers()
        {
            for (int id = 0; id < Capacity; id++)
            {
                if (modifiers[id].Count == 0) continue;
                modifiers[id].Clear();
                MarkDirty(id);
            }
        }

        // 使い回すとき。補正をすべて外す
        // MEMO: 基礎値と範囲は残す(プールから出すときにゲーム側が入れ直す)
        public void Restore()
        {
            ClearModifiers();
        }

        internal bool Contains(ALModifierHandle handle) => FindModifier(handle) >= 0;

        int FindModifier(ALModifierHandle handle)
        {
            if (handle.StatId < 0 || handle.StatId >= Capacity) return -1;
            var list = modifiers[handle.StatId];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Serial == handle.Serial) return i;
            }
            return -1;
        }

        float Calculate(int id)
        {
            float add = 0.0f;
            float percentAdd = 0.0f;
            float multiply = 1.0f;
            float multiplyMin = 1.0f;
            bool hasMultiplyMin = false;
            bool hasOverride = false;
            float overrideValue = 0.0f;
            int overrideSerial = 0;

            var list = modifiers[id];
            for (int i = 0; i < list.Count; i++)
            {
                var m = list[i];
                switch (m.Op)
                {
                    case ALModifierOp.Add: add += m.Value; break;
                    case ALModifierOp.PercentAdd: percentAdd += m.Value; break;
                    case ALModifierOp.Multiply: multiply *= m.Value; break;
                    case ALModifierOp.MultiplyMin:
                        multiplyMin = hasMultiplyMin ? Mathf.Min(multiplyMin, m.Value) : m.Value;
                        hasMultiplyMin = true;
                        break;
                    case ALModifierOp.Override:
                        if (hasOverride == false || m.Serial > overrideSerial)
                        {
                            hasOverride = true;
                            overrideValue = m.Value;
                            overrideSerial = m.Serial;
                        }
                        break;
                    default:
                        Debug.Assert(false, $"ALStatTable: Unknown modifier op. op={m.Op}");
                        break;
                }
            }

            float value = hasOverride
                ? overrideValue
                : (baseValues[id] + add) * (1.0f + percentAdd) * multiply * multiplyMin;
            return Mathf.Clamp(value, minValues[id], maxValues[id]);
        }

        void MarkDirty(int id)
        {
            dirtyFlags[id] = true;
            OnChanged?.Invoke(id);
        }

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        void CheckId(int id)
        {
            Debug.Assert(id >= 0 && id < Capacity, $"ALStatTable: Invalid stat id. id={id}, capacity={Capacity}");
        }
    }
}
#nullable restore
