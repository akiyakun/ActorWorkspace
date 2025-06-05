using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Spine;
using Spine.Unity;
using afl;
using afl.UI.v1;
using afl.UI;
using TMPro;

using ActorWorkspace.InAppDebug;

namespace Project.InAppDebug
{
    public class PrototypeViewer : MonoBehaviour
    {
        public GameObject animationListPanel;
        public AnimationControlFormLogic animationControlFormLogic;
        public GameObject openAssetDialog;

        public SkeletonAnimation skeletonAnimation;

        public IActorAssetDatabase ActorAssetDatabase { get; set; }

        public WorkingActorContext CurrentWorkingActorContext { get; set; }

        void Awake()
        {
            // openAssetDialog.SetActive(false);
        }

        async void Start()
        {
            {
                var group = animationListPanel.Find("UIListView").GetComponent<UIEntityGroup>();
                await group.Initialize(UIContextProvider.Default, this.destroyCancellationToken);
            }

            {
                animationControlFormLogic.OnSpeedValueChanged += OnSpeedValueChanged;
                animationControlFormLogic.OnLoopValueChanged += OnLoopValueChanged;
            }

            {
                var group = openAssetDialog.Find("UIListView").GetComponent<UIEntityGroup>();
                await group.Initialize(UIContextProvider.Default, this.destroyCancellationToken);
            }

            LoadAsset("Assets/AssetData/SpineData/Player/Player_SkeletonData.asset");
        }

        public void OpenAsset()
        {
            openAssetDialog.SetActive(true);

            var listView = openAssetDialog.Find("UIListView").GetComponent<UIListView>();
            listView.Clear();

            foreach (var assetInfo in ActorAssetDatabase.GetAll())
            {
                var entity = listView.AddEntity();
                entity.UserData = assetInfo;
                entity.name = assetInfo.Name;
                entity.GetComponentInChildren<TMP_Text>().text = assetInfo.Name;
            }
        }

        public void OnLoadAsset()
        {
            var listView = openAssetDialog.Find("UIListView").GetComponent<UIListView>();
            if (listView.SelectedEntity == null) return;

            var assetInfo = listView.SelectedEntity.UserData as ActorAssetInfoEx;
            Debug.Assert(assetInfo != null);

            LoadAsset(assetInfo.Path);
        }

        void LoadAsset(string path)
        {
            if (CurrentWorkingActorContext != null)
            {
                CurrentWorkingActorContext.Release();
                CurrentWorkingActorContext = null;
            }

            var skeletonGameObject = SpineHelper.CreateSkeletonGameObjectFromAsset(path);
            CurrentWorkingActorContext = new();
            CurrentWorkingActorContext.GameObject = skeletonGameObject;

            openAssetDialog.SetActive(false);

            {
                var uiListView = animationListPanel.Find("UIListView").GetComponent<UIListView>();
                uiListView.Clear();

                SkeletonData skeletonData = skeletonGameObject.GetComponent<SkeletonAnimation>().Skeleton.Data;
                foreach (Spine.Animation animation in skeletonData.Animations)
                {
                    // Debug.Log("Animation name: " + animation.Name);
                    var obj = uiListView.AddEntity();
                    obj.UserData = animation;
                    obj.GetComponentInChildren<TMP_Text>().text = animation.Name;
                }
            }

            // UIをリセット
            animationControlFormLogic.Setup(loop: animationControlFormLogic.IsLoop);
        }

        public void OnSelectedAnimatin(GameObject sender)
        {
            var animation = sender.GetComponent<UIEntity>().UserData as Spine.Animation;
            var skeletonAnimation = CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            skeletonAnimation.AnimationState.SetAnimation(0, animation: animation, loop: animationControlFormLogic.IsLoop);
        }

        public void OnSpeedValueChanged(float value)
        {
            var skeletonAnimation = CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            TrackEntry trackEntry = skeletonAnimation.AnimationState.GetCurrent(0);
            if (trackEntry == null) return;

            trackEntry.TimeScale = value;
        }

        public void OnLoopValueChanged(bool value)
        {
            var skeletonAnimation = CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            TrackEntry trackEntry = skeletonAnimation.AnimationState.GetCurrent(0);
            if (trackEntry == null) return;

            skeletonAnimation.AnimationState.SetAnimation(0, animation: trackEntry.Animation, loop: value);
        }

    }
}
