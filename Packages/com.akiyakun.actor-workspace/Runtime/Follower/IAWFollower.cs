#nullable enable
using UnityEngine;

namespace ActorWorkspace
{
    public interface IAWFollower
    {
        // アクティブ状態が変化される直前に呼ばれる
        // MEMO: falseになるときに何か処理することがまず無さそうなので一旦オミット
        // public event System.Action<IAWFollower, bool>? OnActiveChange;

        // アクティブ状態が変化される直前に呼ばれる
        public event System.Action<IAWFollower>? OnActivating;
        // public event System.Action<IAWFollower>? OnActivated;
        // public event System.Action<IAWFollower>? OnDeactivating;

        public GameObject GameObject { get; }

    }
}
#nullable restore