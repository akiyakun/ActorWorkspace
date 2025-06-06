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
        public SkinControlFormLogic skinControlFormLogic;
        public AnimationControlFormLogic animationControlFormLogic;
        public GameObject openAssetDialog;

        public IActorAssetDatabase ActorAssetDatabase { get; set; }

        public WorkingActorContext CurrentWorkingActorContext { get; set; }

        int currentTrackIndex = 0;

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
                var group = skinControlFormLogic.gameObject.Find("Root/UIListView").GetComponent<UIEntityGroup>();
                await group.Initialize(UIContextProvider.Default, this.destroyCancellationToken);

                skinControlFormLogic.OnSkinChanged += OnSkinChanged;
            }

            {
                animationControlFormLogic.OnSpeedValueChanged += OnSpeedValueChanged;
                animationControlFormLogic.OnLoopValueChanged += OnLoopValueChanged;
                animationControlFormLogic.OnActiveTrackChanged += OnActiveTrackChanged;
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

            var skeletonAnimation = skeletonGameObject.GetComponent<SkeletonAnimation>();

            {
                var uiListView = animationListPanel.Find("UIListView").GetComponent<UIListView>();
                uiListView.Clear();

                SkeletonData skeletonData = skeletonAnimation.Skeleton.Data;
                // foreach (Spine.Animation animation in skeletonData.Animations)
                for (int i = 0; i < skeletonData.Animations.Count; i++)
                {
                    // Debug.Log("Animation name: " + animation.Name);
                    var animation = skeletonData.Animations.Items[i];
                    var entity = uiListView.AddEntity();
                    entity.Id = i;
                    entity.UserData = animation;
                    entity.GetComponentInChildren<TMP_Text>().text = animation.Name + " id:" + i.ToString();
                }
            }

            // UIをリセット
            animationControlFormLogic.ResetUI(loop: animationControlFormLogic.IsLoop);
            skinControlFormLogic.ResetUI(skeletonAnimation);
        }

        public void OnSelectedAnimation(GameObject sender)
        {
            var animation = sender.GetComponent<UIEntity>().UserData as Spine.Animation;
            var skeletonAnimation = CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();

            TrackEntry cu = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            if (cu != null && cu.Animation == animation)
            {
                // 同じアニメーションが選択された場合は停止させる
                skeletonAnimation.AnimationState.SetEmptyAnimation(currentTrackIndex, 0.0f);
                // ClearTrack()だけだと一時停止みたいになってしまう
                // skeletonAnimation.AnimationState.ClearTrack(currentTrackIndex);
                animationControlFormLogic.TrackAnimationChanged(currentTrackIndex, false);
            }
            else
            {
                skeletonAnimation.AnimationState.SetAnimation(currentTrackIndex, animation: animation, loop: animationControlFormLogic.IsLoop);
                animationControlFormLogic.TrackAnimationChanged(currentTrackIndex, true);
            }
        }

        public void OnSkinChanged(int index)
        {
            var skeletonAnimation = CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            var skeleton = skeletonAnimation.Skeleton;

            if (index < 0 || skeleton.Data.Skins.Count <= index)
            {
                Debug.Assert(false);
                return;
            }

            var skin = skeleton.Data.Skins.Items[index];
            skeleton.SetSkin(skin);
            skeleton.SetSlotsToSetupPose();
        }

        public void OnSpeedValueChanged(float value)
        {
            var skeletonAnimation = CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            TrackEntry trackEntry = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            if (trackEntry == null) return;

            trackEntry.TimeScale = value;
        }

        public void OnLoopValueChanged(bool value)
        {
            var skeletonAnimation = CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            TrackEntry trackEntry = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            if (trackEntry == null) return;

            skeletonAnimation.AnimationState.SetAnimation(currentTrackIndex, animation: trackEntry.Animation, loop: value);
        }

        // index: UIの0-4のボタンのインデックス
        public void OnActiveTrackChanged(int index)
        {
            // Debug.Log($"OnActiveTrackChanged: index={index}");
            currentTrackIndex = index;

            var uiListView = animationListPanel.Find("UIListView").GetComponent<UIListView>();

            var skeletonAnimation = CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            TrackEntry trackEntry = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            if (trackEntry == null)
            {
                afl.Service.Input.EventSystemHelper.SetSelectedGameObject(null);
                return;
            }

            SkeletonData skeletonData = skeletonAnimation.Skeleton.Data;
            // foreach (Spine.Animation animation in skeletonData.Animations)
            for (int i = 0; i < skeletonData.Animations.Count; i++)
            {
                var animation = skeletonData.Animations.Items[i];
                if (animation == trackEntry.Animation)
                {
                    var entity = uiListView.ContentRoot.GetEntity(i);
                    Debug.Assert(entity != null, $"Entity not found for index {i} in UIListView");
                    entity.gameObject.GetComponentInChildren<Button>().Select();
                    return;
                }
            }

            afl.Service.Input.EventSystemHelper.SetSelectedGameObject(null);
        }

    }
}
