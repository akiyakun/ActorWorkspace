using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using afl;
using afl.UI;
using afl.UI.v1;

namespace ActorWorkspace.InAppDebug
{
    public class UIPlayListControl : UIEntityRootGroup
    {
        const int trackIndex = 0;

        [SerializeField] UIListView listView;

        // SkeletonAnimation skeletonAnimation;
        // Spine.Animation lastAnimation;
        IAWActor actor;
        IAWAnimationController animationController;
        IAWAnimation lastAnimation;

        protected override void OnAwake()
        {
            ContextProvider.EventBus.Subscribe(ActorWorkspaceFormEvents.ResetUI, actor => ResetUI(actor));
            // FormBinding.Bus.Subscribe("ResetUI", (IAWActor actor) => ResetUI(actor));

            ContextProvider.EventBus.Subscribe(ActorWorkspaceFormEvents.PlayList_Clear, () => ClearPlayList());
            ContextProvider.EventBus.Subscribe(ActorWorkspaceFormEvents.PlayList_Add, animation => AddPlayList(animation));
        }

        // public void ResetUI(IAWActor actor)
        void ResetUI(IAWActor actor)
        {
            // this.skeletonAnimation = skeletonAnimation;
            // lastAnimation = null;
            this.actor = actor;
            animationController = actor.AnimationController;

            // skeletonAnimation.state.Complete += OnAnimationComplete;
            animationController.OnAnimationComplate += OnAnimationComplete;
        }

        void ClearPlayList()
        {
            listView.Clear();
            lastAnimation = null;

            // skeletonAnimation.state.SetEmptyAnimation(trackIndex, 0.0f);
            animationController.SetEmptyAnimation(trackIndex, 0.0f);
        }

        void AddPlayList(IAWAnimation animation)
        {
            var entity = listView.AddEntity();
            entity.UserData = animation;
            if (entity.gameObject.GetComponent<TMP_Text>("DefaultButton/Text") is var component)
            {
                component.text = animation.Name;
            }
            entity.SetStay();

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
                // var animation = itemList[i].UserData as Spine.Animation;
                var animation = itemList[i].UserData as IAWAnimation;
                if (i == 0)
                {
                    // skeletonAnimation.state.SetAnimation(trackIndex, animation, loop: false);
                    animationController.SetAnimation(trackIndex, animation, loop: false);
                }
                else
                {
                    // skeletonAnimation.state.AddAnimation(trackIndex, animation, loop: false, delay: 0.0f);
                    animationController.AddAnimation(trackIndex, animation, loop: false, delay: 0.0f);
                }
            }
        }

        // void OnAnimationComplete(Spine.TrackEntry entry)
        // {
        //     if (lastAnimation == null) return;

        //     if (lastAnimation == entry.Animation)
        //     {
        //         Debug.Log($"OnAnimationComplete: {entry.Animation.Name}");
        //         ResetAnimations();
        //     }
        // }
        void OnAnimationComplete(IAWAnimation animation)
        {
            if (lastAnimation == null) return;

            if (lastAnimation == animation)
            {
                Debug.Log($"OnAnimationComplete: {animation.Name}");
                ResetAnimations();
            }
        }

        // public void OnClearPlayList()
        // {
        //     ClearPlayList();
        // }

    }
}
