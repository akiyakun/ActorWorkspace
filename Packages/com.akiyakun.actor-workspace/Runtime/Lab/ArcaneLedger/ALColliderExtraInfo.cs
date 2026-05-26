#nullable enable
using UnityEngine;
using afl;
using afl.Service.Input;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // コライダーがアタッチされたオブジェクトから必要な情報を保持・取得するためのコンポーネント
    public class ALColliderExtraInfo : MonoBehaviour
    {
        public IAWActor Actor { get; set; } = null!;
        public IALProcessor? ALProcessor { get; set; } = null!;

        [SerializeField, Disable] uint serialNumber = 0;
        public uint SerialNumber { get => serialNumber; set => serialNumber = value; }

        public object? UserData { get; set; }
    }
}
#nullable restore