#nullable enable
using UnityEngine;
using afl;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger
{
    // コライダーがアタッチされたオブジェクトから必要な情報を保持・取得するためのコンポーネント
    // ALHitBoxBehaviour・ALHurtBoxBehaviour が HitBox・HurtBox のアタッチメントに付ける
    //
    // See also: EMCollisionExtraData.cs
    [DisallowMultipleComponent]
    public class ALCollisionExtraData : MonoBehaviour, IALCollisionExtraData
    {
        [Header("通常コンポーネント版のIALCollisionExtraData実装")]

        #region IALCollisionExtraData
        public IAWActor? Actor { get => actor; set { actor = value; ledger = null; } }
        IAWActor? actor;
        public IALProcessor? ALProcessor { get; set; }

        [SerializeField, Disable] uint serialNumber = 0;
        public uint SerialNumber { get => serialNumber; set => serialNumber = value; }

        [SerializeField] uint flags;
        public uint Flags { get => flags; set => flags = value; }

        [SerializeField] int intValue;
        public int IntValue { get => intValue; set => intValue = value; }

        [SerializeField] float floatValue;
        public float FloatValue { get => floatValue; set => floatValue = value; }

        [SerializeField] string stringValue = string.Empty;
        public string StringValue { get => stringValue; set => stringValue = value; }

        public object? UserData { get; set; }

        // 陣営はアクターの ArcaneLedgerBehaviour から読む(アクターが無ければ 0)
        // MEMO: 当たりの候補を足すたびに読むので、見つけた台帳を覚えておく(Actor を変えたら探し直す。見つからなければ次も探す)
        public int Faction
        {
            get
            {
                ledger ??= actor?.GetLedger();
                return ledger?.Faction ?? 0;
            }
        }
        ArcaneLedgerBehaviour? ledger;

        [SerializeField] int attackId;
        public int AttackId { get => attackId; set => attackId = value; }

        // MEMO: 既定は None(当たるたびに処理する。今までの動き)。ALHitBoxBehaviour.SetDefaultHitHistory で変えられる
        [SerializeField] ALHitHistoryMode hitHistoryMode = ALHitHistoryMode.None;
        public ALHitHistoryMode HitHistoryMode { get => hitHistoryMode; set => hitHistoryMode = value; }

        [SerializeField] float repeatHitInterval;
        public float RepeatHitInterval { get => repeatHitInterval; set => repeatHitInterval = value; }

        [SerializeField] int region;
        public int Region { get => region; set => region = value; }

        [SerializeField] float damageMultiplier = 1.0f;
        public float DamageMultiplier { get => damageMultiplier; set => damageMultiplier = value; }

        [SerializeField] int priority;
        public int Priority { get => priority; set => priority = value; }
        #endregion


#if UNITY_EDITOR
        // [Space(10)]
        [Header("Debug")]
        [SerializeField] bool debugBreakPause = false;
        public bool DebugBreakPause { get => debugBreakPause; set => debugBreakPause = value; }
#endif
    }
}
#nullable restore
