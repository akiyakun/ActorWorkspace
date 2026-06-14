#nullable enable
namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // コライダーのシリアルナンバーを返すクラス
    public static class ColliderSerialNumberGenerator
    {
        static uint currentSerialNumber = 0;
        public static uint GetNext()
        {
            return unchecked((uint)++currentSerialNumber);
        }

    }
}
#nullable restore