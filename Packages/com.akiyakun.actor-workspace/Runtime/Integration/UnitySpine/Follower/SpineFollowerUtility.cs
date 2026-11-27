#nullable enable
using UnityEngine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    // フォローオブジェクト(Spine の骨・ポイントについて回る GameObject)のユーティリティ
    public static class SpineFollowerUtility
    {
        // フォローオブジェクトの BoneFollower・PointFollower を、今の骨の位置にすぐ動かす
        // エフェクトなどをフォローオブジェクトの下で再生する(位置を写す)直前に呼ぶ
        // MEMO: BoneFollower・PointFollower は LateUpdate で動く。それより前(Spine のイベント・Mecanim のアニメーションイベントの中)では、
        //       まだ前のフレームの骨の位置にいる。World 空間の粒は LateUpdate より前(パーティクルの計算の始まり)の位置で出るので、
        //       そのまま再生すると 1 フレーム前の骨の位置に出てしまう
        // MEMO: 骨のワールドの位置が今のフレームのものになってから呼ぶ(Spine の Update のあと)。
        //       このあと本来の LateUpdate でもう一度動くが、同じ位置になるだけ
        // MEMO: BoneSlotFollower(と派生の BoundingBox の Follower)は UpdateComplete で動くので、呼ばなくてよい(何もしない)
        public static void UpdateFollower(Transform follower)
        {
            if (follower.TryGetComponent<BoneFollower>(out var boneFollower))
            {
                if (boneFollower.isActiveAndEnabled) boneFollower.LateUpdate();
                return;
            }
            if (follower.TryGetComponent<PointFollower>(out var pointFollower))
            {
                if (pointFollower.isActiveAndEnabled) pointFollower.LateUpdate();
            }
        }
    }
}
#nullable restore
