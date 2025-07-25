#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace
{
    public interface IAWActor
    {
        public int ActorCategory { get; }
        public GameObject GameObject { get; }

        // 継承先で実装で使用するActorParamを使うだろうから定義しない
        // public ActorParam ActorParam { get; }

        // MEMO: こういう場合良い名前ないですか？
        public IAWActorParam IActorParam { get; }

        // public IAWAnimation Animation { get; }
        public IAWAnimationController AnimationController { get; }
        public IReadOnlyList<IAWSkin> SkinList { get; }

        // 初期状態に戻す
        public void Restore();

        // public IAWActorParam GetActorParam();
        public void SetSkin(int skinIndex);
    }
}
#nullable restore