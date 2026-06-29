#nullable enable
using UnityEngine;
using afl;
using afl.Service.Input;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // コライダーがアタッチされたオブジェクトから必要な情報を保持・取得するためのコンポーネント
    //
    // See also: EMCollisionExtraData.cs
    [DisallowMultipleComponent]
    public class ALCollisionExtraData : MonoBehaviour, IALCollisionExtraData
    {
        [Header("通常コンポーネント版のIALCollisionExtraData実装")]

        #region IALCollisionExtraData
        public IAWActor? Actor { get; set; }
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