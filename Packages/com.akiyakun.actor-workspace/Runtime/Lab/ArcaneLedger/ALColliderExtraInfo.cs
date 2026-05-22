#nullable enable
using UnityEngine;
using afl;
using afl.Service.Input;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    public class ALColliderExtraInfo : MonoBehaviour
    {
        public IAWActor Actor { get; set; } = null!;

        [SerializeField, Disable] uint serialNumber = 0;
        public uint SerialNumber { get => serialNumber; set => serialNumber = value; }

        public object? UserData { get; set; }
    }
}
#nullable restore