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

        IAWAnimationController animationController;
        IAWAnimation lastAnimation;

        protected override void OnAwake()
        {
            ContextProvider.EventBus.Subscribe(ActorWorkspaceFormEvents.ResetUI, actor => ResetUI(actor));
            ContextProvider.EventBus.Subscribe(ActorWorkspaceFormEvents.PlayList_Add, animation => AddPlayList(animation));
        }

        protected override void InnerTerminate()
        {
            UnbindAnimationController();
        }

        // アクターを読み直したとき
        void ResetUI(IAWActor actor)
        {
            UnbindAnimationController();

            // 前のアクターのアニメーションなので、再生リストを空にする
            listView.Clear();
            lastAnimation = null;

            animationController = actor.AnimationController;
            animationController.OnAnimationComplete += OnAnimationComplete;
        }

        void UnbindAnimationController()
        {
            if (animationController == null) return;
            animationController.OnAnimationComplete -= OnAnimationComplete;
            animationController = null;
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

            lastAnimation = animation;

            ResetAnimations();
        }

        void ResetAnimations()
        {
            var itemList = listView.ItemList;
            for (int i = 0; i < itemList.Count; i++)
            {
                var animation = itemList[i].UserData as IAWAnimation;
                if (i == 0)
                {
                    animationController.SetAnimation(Utility.StringToHashId(animation.Name),
                        new AWAnimationOption { Track = trackIndex, Flags = (uint)AWAnimationOptionFlag.Loop });
                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }

        void OnAnimationComplete(IAWAnimation animation)
        {
            if (lastAnimation == null) return;

            if (lastAnimation == animation)
            {
                Debug.Log($"OnAnimationComplete: {animation.Name}");
                ResetAnimations();
            }
        }
    }
}
