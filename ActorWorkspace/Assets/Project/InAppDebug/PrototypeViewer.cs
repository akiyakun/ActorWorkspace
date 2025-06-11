using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using Spine;
using Spine.Unity;
using afl;
using afl.UI.v1;
using afl.UI;
using TMPro;

using ActorWorkspace.InAppDebug;
using UnityEditor;

namespace Project.InAppDebug
{
    public class PrototypeViewer : MonoBehaviour
    {
        public GameObject animationListPanel;
        public AnimationListFormLogic animationListFormLogic;
        public SkinControlFormLogic skinControlFormLogic;
        public AnimationControlFormLogic animationControlFormLogic;
        public PlayListControlFormLogic playListControlFormLogic;
        public GameObject openAssetDialog;

        public IActorAssetDatabase ActorAssetDatabase { get; set; }

        public WorkingActorContext CurrentWorkingActorContext { get; set; }

        int currentTrackIndex = 0;

        void Awake()
        {
            // openAssetDialog.SetActive(false);
        }

        // async void Start()
        public async UniTask InitAsync()
        {
            {
                var group = animationListPanel.Find("UIListView").GetComponent<UIEntityGroup>();
                await group.Initialize(UIContextProvider.Default, this.destroyCancellationToken);

                animationListFormLogic.OnClickEntity += OnClickEntityFromAnimationList;
            }

            {
                var group = skinControlFormLogic.gameObject.Find("Root/UIListView").GetComponent<UIEntityGroup>();
                await group.Initialize(UIContextProvider.Default, this.destroyCancellationToken);

                skinControlFormLogic.OnSkinChanged += OnSkinChanged;
            }

            {
                animationControlFormLogic.OnSpeedValueChanged += OnSpeedValueChanged;
                animationControlFormLogic.OnMixValueChanged += OnMixValueChanged;
                animationControlFormLogic.OnLoopValueChanged += OnLoopValueChanged;
                animationControlFormLogic.OnActiveTrackChanged += OnActiveTrackChanged;
            }

            {
                var group = playListControlFormLogic.gameObject.Find("Root/UIListView").GetComponent<UIEntityGroup>();
                await group.Initialize(UIContextProvider.Default, this.destroyCancellationToken);
            }

            {
                var group = openAssetDialog.Find("UIListView").GetComponent<UIEntityGroup>();
                await group.Initialize(UIContextProvider.Default, this.destroyCancellationToken);
            }

            // LoadAsset("Assets/AssetData/SpineData/Player/Player_SkeletonData.asset");
            LoadAsset("Assets/AssetData/SpineData/Kioni/Kioni_SkeletonData.asset");
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

            var assetInfo = listView.SelectedEntity.UserData as ActorAssetInfo;
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

            SkeletonAnimation skeletonAnimation = null;

            skeletonAnimation = ActorAssetDatabase.CreateActorAsset(path).GetComponent<SkeletonAnimation>();

            CurrentWorkingActorContext = new();
            CurrentWorkingActorContext.GameObject = skeletonAnimation.gameObject;

            openAssetDialog.SetActive(false);

            // UIをリセット
            animationListFormLogic.ResetUI(skeletonAnimation);
            animationControlFormLogic.ResetUI(loop: animationControlFormLogic.IsLoop);
            skinControlFormLogic.ResetUI(skeletonAnimation);
            playListControlFormLogic.ResetUI(skeletonAnimation);
        }

        // アニメーションリストからアニメーションをクリックしたときの処理
        public void OnClickEntityFromAnimationList(UIEntity sender)
        {
            var animation = sender.UserData as Spine.Animation;
            var skeletonAnimation = CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();

            // Ctrlが押されている場合は再生リストに追加する
            if (Keyboard.current != null && Keyboard.current.ctrlKey.isPressed)
            {
                playListControlFormLogic.AddPlayList(animation);
                return;
            }

            TrackEntry cu = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            if (cu != null && cu.Animation == animation)
            {
                // 再生チェックマークを更新
                animationListFormLogic.SetPlayingCheckmark(sender, currentTrackIndex, false);

                // 同じアニメーションが選択された場合は停止させる
                skeletonAnimation.AnimationState.SetEmptyAnimation(currentTrackIndex, 0.0f);
                // ClearTrack()だけだと一時停止みたいになってしまう
                // skeletonAnimation.AnimationState.ClearTrack(currentTrackIndex);
                animationControlFormLogic.TrackAnimationChanged(currentTrackIndex, false);
            }
            else
            {
                // 再生チェックマークを更新
                if (animationListFormLogic.SetPlayingCheckmark(sender, currentTrackIndex, true) == false)
                {
                    // 別のトラックが再生中なので再生ができない
                    // 停止させる
                    animationListFormLogic.SetPlayingCheckmark(sender, currentTrackIndex, false);
                    skeletonAnimation.AnimationState.SetEmptyAnimation(currentTrackIndex, 0.0f);
                    animationControlFormLogic.TrackAnimationChanged(currentTrackIndex, false);
                    return;
                }

                // if (cu != null && cu.Animation != null)
                // {
                //     var stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();
                //     stateData.SetMix(cu.Animation, animation, stateData.DefaultMix);
                // }

                TrackEntry trackEntry = skeletonAnimation.AnimationState.SetAnimation(
                    currentTrackIndex, animation: animation, loop: animationControlFormLogic.IsLoop);
                // MixDurationを0にしないとDefaultMixが適応されない?
                // trackEntry.MixDuration = 3.0f;

                // skeletonAnimation.AnimationState.AddAnimation(
                //     currentTrackIndex, animation: animation, loop: animationControlFormLogic.IsLoop, delay: 0.0f);

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
            // Debug.Log($"OnSpeedValueChanged: trackEntry.TimeScale={trackEntry.TimeScale}, value={value}");
        }

        public void OnMixValueChanged(float value)
        {
            var skeletonAnimation = CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            // TrackごとにMixを設定するようなことはできない
            AnimationStateData stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();
            stateData.DefaultMix = value;
            // Debug.Log($"OnMixValueChanged: stateData.DefaultMix={stateData.DefaultMix}, value={value}");
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
