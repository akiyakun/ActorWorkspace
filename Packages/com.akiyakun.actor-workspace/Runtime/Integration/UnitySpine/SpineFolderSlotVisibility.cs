#nullable enable

namespace ActorWorkspace.UnitySpine
{
    // Spine のフォルダ(3_CollisionBoxFolder など)の判定の箱の、目印の画像(スロットの絵)を出すか
    // ふだんは出さない。デバッグで当たり判定を見たいときに true にする(次の更新から反映される)
    // 値を入れていないときは UnitySpineSettings の Show Folder Slots を使う(Play 中に Inspector で変えても反映される)
    // See also: BoneSlotFollower.cs(hideSlot), ExtraDataUtility.cs
    public static class SpineFolderSlotVisibility
    {
        static bool? isVisible;

        public static bool IsVisible
        {
            get => isVisible ?? UnitySpineSettings.Instance.ShowFolderSlots;
            set => isVisible = value;
        }

        // 入れた値を消し、UnitySpineSettings の値に戻す
        public static void Reset() => isVisible = null;
    }
}
#nullable restore
