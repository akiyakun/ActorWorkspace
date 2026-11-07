#nullable enable
namespace ActorWorkspace
{
    // 再生速度(時間の倍率)を変えられるアニメーションコントローラー
    // MEMO: 自分の Update で進むもの(Spine の SkeletonAnimation や Animator)は、
    //       IAWAnimationController.DoUpdate() に渡す時間を変えても速さが変わらないので、これで変える
    public interface IAWTimeScalable
    {
        // 1 が通常の速さ
        public float TimeScale { get; set; }
    }
}
#nullable restore
