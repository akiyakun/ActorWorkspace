#nullable enable
using UnityEngine;
using afl;
using afl.Service.Input;
using ActorWorkspace;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // コライダーがアタッチされたオブジェクトから必要な情報を保持・取得するためのインターフェース
    public interface IALColliderAttachmentData
    {
        public IAWActor? Actor { get; }
        public IALProcessor? ALProcessor { get; }

        public uint SerialNumber { get; }

        public int IntValue { get; }
        public float FloatValue { get; }
        public string StringValue { get; }
        public object? UserData { get; }
    }
}
#nullable restore