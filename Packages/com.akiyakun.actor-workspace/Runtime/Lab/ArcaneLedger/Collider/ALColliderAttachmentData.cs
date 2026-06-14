#nullable enable
using UnityEngine;
using afl;
using afl.Service.Input;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // コライダーがアタッチされたオブジェクトから必要な情報を保持・取得するためのコンポーネント
    public class ALColliderAttachmentData : MonoBehaviour, IALColliderAttachmentData
    {
        #region IALColliderAttachmentData
        public IAWActor? Actor { get; set; }
        public IALProcessor? ALProcessor { get; set; }

        [SerializeField, Disable] uint serialNumber = 0;
        public uint SerialNumber { get => serialNumber; set => serialNumber = value; }

        [SerializeField] int intValue;
        public int IntValue { get => intValue; set => intValue = value; }

        [SerializeField] float floatValue;
        public float FloatValue { get => floatValue; set => floatValue = value; }

        [SerializeField] string stringValue = string.Empty;
        public string StringValue { get => stringValue; set => stringValue = value; }

        public object? UserData { get; set; }
        #endregion
    }
}
#nullable restore