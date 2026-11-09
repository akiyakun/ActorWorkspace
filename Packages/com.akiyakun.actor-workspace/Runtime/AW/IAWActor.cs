#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public interface IAWActor : IUpdateElement, System.IDisposable
    {
        // FIXME:
        public bool IsInView { get; set; }

        public int ActorId { get; }
        public int ActorCategory { get; }
        public GameObject GameObject { get; }

        // public AWActorContextProvider ActorContextProvider { get; }

        public IAWActorParam ActorParam { get; }
        public AWActorDisplay ActorDisplay { get; }
        public IAWAnimationController AnimationController { get; }
        public IReadOnlyList<IAWSkin> SkinList { get; }
        // 今付けているスキンの番号(SkinList の番号)。合成しているときは合成した順
        // SkinList に無いスキンを付けているときは空
        public IReadOnlyList<int> SkinIndices { get; }
        public AWActorBehaviourController ActorBehaviourController { get; }
        public EventBus<string> EventBus { get; }
        public VariableTable Variables { get; }

        // public IALProcessor? ALProcessor { get; }

        // 初期状態に戻す
        public void Restore();

        // public IAWActorParam GetActorParam();
        public void SetSkin(int skinIndex);
        // 複数のスキンを合成して付ける。同じスロットの同じ名前のアタッチメントは、後ろのスキンのものを使う
        // MEMO: Spine のアクターは、スキンを変えたあと、次にアニメーションを当てる直前にスロットをセットアップポーズに戻す。
        // それまでにスケルトンを直接変えた内容(アタッチメント・色など)は消える
        public void SetSkins(IReadOnlyList<int> skinIndices);
    }
}
#nullable restore