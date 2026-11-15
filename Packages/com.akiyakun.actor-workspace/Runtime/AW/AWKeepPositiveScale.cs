#nullable enable
using UnityEngine;

namespace ActorWorkspace
{
    // 親の反転(マイナスのスケール)を打ち消して、ワールドのスケール(lossyScale)をプラスに保つ
    // AWActorDisplay の子の 3D の BoxCollider に付ける。3D の BoxCollider はマイナスのスケールに対応しておらず、
    // トリガーでなく kinematic でない Rigidbody に付いていると、物理の更新で警告が出るため(2D の BoxCollider2D は出ない)
    // MEMO: AWActorDisplay が反転するたびに Apply() を呼ぶ。反転したその場で直さないと、次の物理の更新に間に合わないことがある
    // MEMO: 自分の向きは反転しなくなるので、BoxCollider.center のずれは左右反転しない。center は 0 にして、位置は GameObject で合わせる
    public class AWKeepPositiveScale : MonoBehaviour
    {
        public void Apply()
        {
            var parent = transform.parent;
            if (parent == null) return;

            var parentScale = parent.lossyScale;
            var scale = transform.localScale;
            var newScale = new Vector3(
                MatchSign(scale.x, parentScale.x),
                MatchSign(scale.y, parentScale.y),
                MatchSign(scale.z, parentScale.z));

            if (newScale != scale) transform.localScale = newScale;
        }

        // 親のスケールと同じ符号にする(掛けるとプラスになる)
        static float MatchSign(float value, float parentValue)
        {
            return parentValue < 0.0f ? -Mathf.Abs(value) : Mathf.Abs(value);
        }
    }
}
#nullable restore
