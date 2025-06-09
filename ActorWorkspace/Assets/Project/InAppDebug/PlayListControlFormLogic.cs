using UnityEngine;
using UnityEngine.UI;
using TMPro;
using afl;
using afl.UI.v1;

using Spine.Unity;

namespace Project.InAppDebug
{
    public class PlayListControlFormLogic : MonoBehaviour
    {
        const int trackIndex = 0;

        [SerializeField] UIListView listView;

        SkeletonAnimation skeletonAnimation;
        Spine.Animation lastAnimation;

        public void ResetUI(SkeletonAnimation skeletonAnimation)
        {
            this.skeletonAnimation = skeletonAnimation;
            lastAnimation = null;

            skeletonAnimation.state.Complete += OnAnimationComplete;
        }

        public void AddPlayList(Spine.Animation animation)
        {
            var entity = listView.AddEntity();
            entity.UserData = animation;

            entity.gameObject.Find("DefaultButton/Text").GetComponent<TMP_Text>().text = animation.Name;

            // var state = skeletonAnimation.state;
            // if (listView.ItemCount == 1)
            // {
            //     state.SetAnimation(trackIndex, animation, loop: false);
            // }
            // else
            // {
            //     state.AddAnimation(trackIndex, animation, loop: false, delay: 0.0f);
            // }

            lastAnimation = animation;

            ResetAnimations();
        }

        void ResetAnimations()
        {
            var itemList = listView.ItemList;
            for (int i = 0; i < itemList.Count; i++)
            {
                var animation = itemList[i].UserData as Spine.Animation;
                if (i == 0)
                {
                    skeletonAnimation.state.SetAnimation(trackIndex, animation, loop: false);
                }
                else
                {
                    skeletonAnimation.state.AddAnimation(trackIndex, animation, loop: false, delay: 0.0f);
                }
            }
        }

        void OnAnimationComplete(Spine.TrackEntry entry)
        {
            if (lastAnimation == null) return;

            if (lastAnimation == entry.Animation)
            {
                Debug.Log($"OnAnimationComplete: {entry.Animation.Name}");
                ResetAnimations();
            }
        }

    }
}
