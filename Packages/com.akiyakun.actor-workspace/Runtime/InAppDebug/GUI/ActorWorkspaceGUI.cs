using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Threading;
using Cysharp.Threading.Tasks;
using afl;
using afl.UI.v1;
using afl.UI;
using TMPro;
using ActorWorkspace.MasterData;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceGUI : MonoBehaviour, IAsyncInitializable
    {
        public const string AssetRootDirectory = "Assets/AssetBundleData/Actor";

        [SerializeField] Camera navigationCamera;
        public Camera CurrentCamera => navigationCamera;

        public AnimationListFormLogic animationListFormLogic;
        public SkinControlFormLogic skinControlFormLogic;
        public AnimationControlFormLogic animationControlFormLogic;
        public PlayListControlFormLogic playListControlFormLogic;
        public GameObject openAssetDialog;

        // [SerializeField] SerializableInterface<IActorAssetCollection> actorAssetDatabase;
        // public IActorAssetCollection ActorAssetDatabase
        // {
        //     get => actorAssetDatabase.Interface;
        //     set => actorAssetDatabase.Interface = value;
        // }
        public ActorWorkspaceGUIContextProvider ContextProvider { get; private set; }
        // public WorkingActorContext CurrentWorkingActorContext { get; set; }

        int currentTrackIndex = 0;

        class TrackInfo
        {
            public float Speed = 1.0f;
            public float Mix = 0.25f;
        }
        List<TrackInfo> trackInfoList = ListEx.Create<TrackInfo>(16);

        // From IAsyncInitializable
        public bool IsInitialized { get; protected set; }

        // void Awake()
        // {
        //     // openAssetDialog.SetActive(false);
        // }

        public void SetContextProvider(ActorWorkspaceGUIContextProvider contextProvider)
        {
            ContextProvider = contextProvider;
        }

        // From IAsyncInitializable
        public async UniTask<int> InitializeAsync(CancellationToken cancellationToken = default)
        {
            // ContextProvider = contextProvider;
            Debug.Assert(ContextProvider != null);

            {
                var group = animationListFormLogic.gameObject.Find("UIListView").GetComponent<UIEntityGroup>();
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

#if UNITY_EDITOR
            await InitializeEditorAsync(cancellationToken);
#endif

            IsInitialized = true;
            return 0;
        }

        // From IAsyncInitializable
        public async UniTask TerminateAsync(CancellationToken cancellationToken = default)
        {
            await UniTask.Yield();
        }

        public void OpenAsset()
        {
            openAssetDialog.SetActive(true);

            var listView = openAssetDialog.Find("UIListView").GetComponent<UIListView>();
            listView.Clear();

            var database = ContextProvider.AssetRepository.ToList();
            foreach (var model in database)
            {
                ActorAssetInfo info = model as ActorAssetInfo;
                var entity = listView.AddEntity();
                entity.UserData = info;
                entity.name = info.Name;
                entity.GetComponentInChildren<TMP_Text>().text = info.Name;
            }
        }

        public void OnLoadAsset()
        {
            var listView = openAssetDialog.Find("UIListView").GetComponent<UIListView>();
            if (listView.SelectedEntity == null) return;

            var assetInfo = listView.SelectedEntity.UserData as ActorAssetInfo;
            Debug.Assert(assetInfo != null);

            LoadAsset(assetInfo.Id).Forget();
        }

        async UniTask LoadAsset(int id)
        {
            if (ContextProvider.CurrentWorkingActorContext != null)
            {
                ContextProvider.CurrentWorkingActorContext.Release();
                ContextProvider.CurrentWorkingActorContext = null;
            }

            // SkeletonAnimation skeletonAnimation = null;

            // skeletonAnimation = ActorAssetDatabase.CreateActorAsset(assetLocator).GetComponent<SkeletonAnimation>();
            IAWActor actor = await ContextProvider.ActorFactory.CreateAsync(id);
            // skeletonAnimation = actor.GameObject.GetComponent<SkeletonAnimation>();

            ContextProvider.CurrentWorkingActorContext = new();
            // ContextProvider.CurrentWorkingActorContext.GameObject = skeletonAnimation.gameObject;
            ContextProvider.CurrentWorkingActorContext.Actor = actor;

            openAssetDialog.SetActive(false);

            // UIをリセット
            animationListFormLogic.ResetUI(actor);
            animationControlFormLogic.ResetUI(loop: animationControlFormLogic.IsLoop);
            skinControlFormLogic.ResetUI(actor);
            playListControlFormLogic.ResetUI(actor);
        }


        // アニメーションリストからアニメーションをクリックしたときの処理
        public void OnClickEntityFromAnimationList(UIEntity sender)
        {
            // var animation = sender.UserData as Spine.Animation;
            // var skeletonAnimation = ContextProvider.CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            var animation = sender.UserData as IAWAnimation;
            var animationController = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController;

            // Ctrlが押されている場合は再生リストに追加する
            if (Keyboard.current != null && Keyboard.current.ctrlKey.isPressed)
            {
                playListControlFormLogic.AddPlayList(animation);
                return;
            }

            // AnimationStateData stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();

            // TrackEntry cu = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            // if (cu != null && cu.Animation == animation)
            IAWTrack cu = animationController.GetTrack(currentTrackIndex);
            if (cu != null && cu.Animation == animation)
            {
                // 再生チェックマークを更新
                animationListFormLogic.SetPlayingCheckmark(sender, currentTrackIndex, false);

                // 同じアニメーションが選択された場合は停止させる
                // animationController.SetEmptyAnimation(currentTrackIndex, stateData.DefaultMix);
                animationController.SetEmptyAnimation(currentTrackIndex);

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
                    // animationController.SetEmptyAnimation(currentTrackIndex, stateData.DefaultMix);
                    animationController.SetEmptyAnimation(currentTrackIndex);
                    animationControlFormLogic.TrackAnimationChanged(currentTrackIndex, false);
                    return;
                }

                // if (cu != null && cu.Animation != null)
                // {
                //     var stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();
                //     stateData.SetMix(cu.Animation, animation, stateData.DefaultMix);
                // }

                // TrackEntry trackEntry =
                var track = animationController.SetAnimation(
                    currentTrackIndex, animation: animation, loop: animationControlFormLogic.IsLoop);
                // MixDurationを0にしないとDefaultMixが適応されない?
                // trackEntry.MixDuration = 3.0f;

                // skeletonAnimation.AnimationState.AddAnimation(
                //     currentTrackIndex, animation: animation, loop: animationControlFormLogic.IsLoop, delay: 0.0f);

                track.TimeScale = trackInfoList[currentTrackIndex].Speed;

                animationControlFormLogic.TrackAnimationChanged(currentTrackIndex, true);

            }

        }

        public void OnSkinChanged(int index)
        {
            /* fixme
            var skeletonAnimation = ContextProvider.CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            var skeleton = skeletonAnimation.Skeleton;

            if (index < 0 || skeleton.Data.Skins.Count <= index)
            {
                Debug.Assert(false);
                return;
            }

            var skin = skeleton.Data.Skins.Items[index];
            skeleton.SetSkin(skin);
            skeleton.SetSlotsToSetupPose();
            */
        }

        public void OnSpeedValueChanged(float value)
        {
            // var skeletonAnimation = ContextProvider.CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            // TrackEntry trackEntry = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            var track = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController.GetTrack(currentTrackIndex);
            if (track == null) return;
            track.TimeScale = value;
            trackInfoList[currentTrackIndex].Speed = value;
            // Debug.Log($"OnSpeedValueChanged: trackEntry.TimeScale={trackEntry.TimeScale}, value={value}");
        }

        public void OnMixValueChanged(float value)
        {
            // var skeletonAnimation = ContextProvider.CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            var track = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController.GetTrack(currentTrackIndex);
            // TrackごとにMixを設定するようなことはできない
            // AnimationStateData stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();
            // stateData.DefaultMix = value;
            track.MixDuration = value;
            trackInfoList[currentTrackIndex].Mix = value;
            // Debug.Log($"OnMixValueChanged: stateData.DefaultMix={stateData.DefaultMix}, value={value}");
        }

        public void OnLoopValueChanged(bool value)
        {
            // var skeletonAnimation = ContextProvider.CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            // TrackEntry trackEntry = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            // if (trackEntry == null) return;
            var animationController = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController;
            var track = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController.GetTrack(currentTrackIndex);

            animationController.SetAnimation(currentTrackIndex, animation: track.Animation, loop: value);
        }

        // index: UIの0-4のボタンのインデックス
        public void OnActiveTrackChanged(int index)
        {
            // Debug.Log($"OnActiveTrackChanged: index={index}");
            currentTrackIndex = index;

            // UI変更
            animationControlFormLogic.ChangeTrack(
                trackInfoList[currentTrackIndex].Speed, trackInfoList[currentTrackIndex].Mix);

            var uiListView = animationListFormLogic.gameObject.Find("UIListView").GetComponent<UIListView>();

            // var skeletonAnimation = ContextProvider.CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            // TrackEntry trackEntry = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            var track = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController.GetTrack(currentTrackIndex);
            if (track == null)
            {
                afl.Service.Input.EventSystemHelper.SetSelectedGameObject(null);
                return;
            }

            // SkeletonData skeletonData = skeletonAnimation.Skeleton.Data;
            // for (int i = 0; i < skeletonData.Animations.Count; i++)
            var animationList = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController.AnimationList;
            for (int i = 0; i < animationList.Count; i++)
            {
                var animation = animationList[i];
                if (animation == track.Animation)
                {
                    var entity = uiListView.ContentRoot.GetEntity(i);
                    Debug.Assert(entity != null, $"Entity not found for index {i} in UIListView");
                    entity.gameObject.GetComponentInChildren<Button>().Select();

                    return;
                }
            }

            afl.Service.Input.EventSystemHelper.SetSelectedGameObject(null);
        }


#if UNITY_EDITOR
        [Space(10)]
        [Header("Editor Debug Only")]

        [SerializeField] string 実行時に読み込むアセットフォルダーのパス;
        string ForceLoadAssetAtRunning
        {
            get => 実行時に読み込むアセットフォルダーのパス;
            set => 実行時に読み込むアセットフォルダーのパス = value;
        }

        async UniTask InitializeEditorAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(ForceLoadAssetAtRunning)) return;

            // var path = AssetDatabase.GetAssetPath(ForceLoadAssetAtRunning);
            var path = Utility.GetDirectoryPath(ForceLoadAssetAtRunning);
            Debug.Log($"ForceLoadAssetAtRunning: {path}");

            // if (string.IsNullOrEmpty(ForceLoadAssetAtRunning) == false)
            {
                var list = ContextProvider.AssetRepository.ToList();
                for (int i = 0; i < list.Count; ++i)
                {
                    var model = list[i];
                    // var path = Utility.GetDirectoryPath(model.GetName()).Replace(ActorWorkspaceGUIContextProvider.AssetRootDirectory, "");
                    // var path = Utility.GetDirectoryPath(model.GetName()).Replace(ActorWorkspaceGUIContextProvider.AssetRootDirectory, "");
                    // if (path == ForceLoadAssetAtRunning)
                    string modelFullPath = Utility.GetDirectoryPath(Utility.PathCombine(ActorWorkspaceGUI.AssetRootDirectory, model.GetName()));
                    Debug.Log($"modelFullPath: {modelFullPath}");
                    if (path == modelFullPath)
                    {
                        await LoadAsset(model.GetId());
                        return;
                    }
                }
            }
        }
#endif
    }
}
