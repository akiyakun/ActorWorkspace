using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Threading;
using Cysharp.Threading.Tasks;
using afl;
using afl.MasterData;
using afl.UI.v1;
using afl.UI;
using TMPro;
using ActorWorkspace.MasterData;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceFormLogic : UIFormLogic<ActorWorkspaceFormContextProvider>
    {
        public const string AssetRootDirectory = "Assets/AssetBundleData/Actor";
        public const string AnimationListControlPath = "Root/AnimationListControl";

        [SerializeField] Camera actorCamera;
        public Camera ActorCamera => actorCamera;

        // [SerializeField] Camera uiCamera;
        // public Camera UICamera => uiCamera;

        // public Camera CurrentCamera => navigationCamera;

        UIAnimationList uiAnimationList;
        UISkinControl uiSkinControl;
        UIAnimationControl uiAnimationControl;
        UIPlayListControl uiPlayListControl;
        public GameObject openAssetDialog;

        bool loadActorRequesting;
        int currentTrackIndex = 0;

        class TrackInfo
        {
            public float Speed = 1.0f;
            public float Mix = 0.25f;
        }
        List<TrackInfo> trackInfoList = ListEx.Create<TrackInfo>(16);

        // From IAsyncInitializable
        // public bool IsInitialized { get; protected set; }

        // void Awake()
        // {
        //     // openAssetDialog.SetActive(false);
        // }

        public void SetContextProvider(ActorWorkspaceFormContextProvider contextProvider)
        {
            ContextProvider = contextProvider;
        }

        // From IAsyncInitializable
        // public async UniTask<int> InitializeAsync(CancellationToken cancellationToken = default)
        // From UIFormLogic
        protected override async UniTask<int> InnerInitializeAsync(CancellationToken cancellationToken)
        {
            // ContextProvider = contextProvider;
            Debug.Assert(ContextProvider != null, "先に SetContextProvider() を呼び出してください。");

            // アクターカメラの設定
            {
                // var originCamera = transform.Find("Root/ActorCamera").GetComponent<Camera>();
                // if (ActorCamera != originCamera)
                // {
                //     // 別のカメラが設定されているときオリジナルは非表示にする
                //     originCamera.gameObject.SetActive(false);
                // }

                var cameraControlArea = Form.Root.transform.Find("CameraControlArea").GetComponent<CameraControlArea>();
                cameraControlArea.SetSourceCamera(ActorCamera);

            }

            // UIカメラの設定
            // {
            //     var originCamera = transform.Find("Root/UICamera").GetComponent<Camera>();
            //     if (UICamera != originCamera)
            //     {
            //         // 別のカメラが設定されているときオリジナルは非表示にする
            //         originCamera.gameObject.SetActive(false);
            //     }

            //     // var canvas = transform.Find("Root/Canvas").GetComponent<Canvas>();
            //     // canvas.worldCamera = UICamera;
            // }

            {
                uiAnimationList = Form.Root.transform.Find("UIAnimationList").GetComponent<UIAnimationList>();
                // var group = animationListFormLogic.transform.Find("UIListView").GetComponent<UIEntityGroup>();
                // await group.InitializeAsync(UIContextProvider.Default, this.destroyCancellationToken);
                // if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Cancel;

                uiAnimationList.OnClickEntity += OnClickEntityFromAnimationList;
            }

            {
                uiSkinControl = Form.Root.transform.Find("UISkinControl").GetComponent<UISkinControl>();
                // var group = uiSkinControl.gameObject.Find("Root/UIListView").GetComponent<UIEntityGroup>();
                // await group.InitializeAsync(UIContextProvider.Default, this.destroyCancellationToken);
                // if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Cancel;

                uiSkinControl.OnSkinChanged += OnSkinChanged;
            }

            {
                uiAnimationControl = Form.Root.transform.Find("UIAnimationControl").GetComponent<UIAnimationControl>();

                uiAnimationControl.OnSpeedValueChanged += OnSpeedValueChanged;
                uiAnimationControl.OnMixValueChanged += OnMixValueChanged;
                uiAnimationControl.OnLoopValueChanged += OnLoopValueChanged;
                uiAnimationControl.OnActiveTrackChanged += OnActiveTrackChanged;
            }

            {
                uiPlayListControl = Form.Root.transform.Find("UIPlayListControl").GetComponent<UIPlayListControl>();

                // var group = uiPlayListControl.gameObject.Find("Root/UIListView").GetComponent<UIEntityGroup>();
                // await group.InitializeAsync(UIContextProvider.Default, this.destroyCancellationToken);
                // if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Cancel;
            }

            // {
            //     var group = openAssetDialog.Find("UIListView").GetComponent<UIEntityGroup>();
            //     await group.InitializeAsync(UIContextProvider.Default, this.destroyCancellationToken);
            //     if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Cancel;
            // }

            // var form = gameObject.GetComponent<IUIForm>();
            // int ret = await form.InitializeAsync(cancellationToken: cancellationToken);
            // if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Cancel;
            // if (ret < 0) return ret;

            // #if UNITY_EDITOR
            //             await InitializeEditorAsync(cancellationToken: cancellationToken);
            //             if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Cancel;
            // #endif

            Debug.Log("ActorWorkspaceGUI initialized successfully.");

            // IsInitialized = true;
            return await UniTask.FromResult(GeneralReturnCode.Success);
        }

        // From IAsyncInitializable
        // public void Terminate()
        // {
        // }
        // From UIFormLogic
        protected override void InnerTerminate()
        {
        }

        // 全てのアセットのリストを生成して返します
        public List<ActorAssetInfo> GetAllAssetList()
        {
            List<ActorAssetInfo> list = new();

            for (int categoty = 0; categoty < ContextProvider.AssetRepositories.Count; ++categoty)
            {
                var assetRepository = ContextProvider.AssetRepositories.Get(categoty);
                Debug.Assert(assetRepository != null);

                IReadOnlyList<IModel> database = assetRepository.ToList();
                for (int i = 0; i < database.Count; ++i)
                {
                    var model = database[i] as AssetModel;
                    Debug.Assert(model != null);

                    var info = new ActorAssetInfo();
                    info.AssetModel = model as AssetModel;
                    info.Category = categoty;

                    info.Name = model.GetName();
                    // Debug.Log($"AssetModel: {info.Name}");
                    if (string.IsNullOrEmpty(info.Name))
                    {
                        // info.Name = System.IO.Path.GetDirectoryName(info.AssetModel.AssetLocator).Replace(assetRootDirectory, "");
                        // info.Name = System.IO.Path.GetFileNameWithoutExtension(info.AssetModel.AssetLocator);
                        info.Name = info.AssetModel.AssetLocator;
                    }

                    list.Add(info);
                }
            }

            return list;
        }


        public void OpenAsset()
        {
            var listView = openAssetDialog.Find("UIListView").GetComponent<UIListView>();

            // リストをクリア
            listView.Clear();

            // UIリストを作成
            var allList = GetAllAssetList();
            foreach (var info in allList)
            {
                var entity = listView.AddEntity();
                entity.UserData = info;
                entity.name = info.Name;
                entity.GetComponentInChildren<TMP_Text>().text = info.Name;
                // entity.OnEntityEvent(UIEntityEvent.Type.Open);
                entity.SetStay();
            }

            openAssetDialog.SetActive(true);
            openAssetDialog.GetComponent<UIEntityGroup>().SendEntityEvent(UIEntityEvent.Type.Open);
        }

        public void OnLoadAsset()
        {
            var listView = openAssetDialog.Find("UIListView").GetComponent<UIListView>();
            if (listView.SelectedEntity == null) return;

            var assetInfo = listView.SelectedEntity.UserData as ActorAssetInfo;
            Debug.Assert(assetInfo != null);

            LoadActorRequest(assetInfo.AssetModel.Id, assetInfo.Category);

            openAssetDialog.GetComponent<UIEntityGroup>().SendEntityEvent(UIEntityEvent.Type.Close);
            openAssetDialog.SetActive(false);
        }

        public void LoadActorRequest(int id, int category)
        {
            LoadActorAsync(id, category, destroyCancellationToken).Forget();
        }

        public async UniTask LoadActorAsync(int id, int category, CancellationToken cancellationToken)
        {
            if (loadActorRequesting == true)
            {
                Debug.Assert(loadActorRequesting == false);
                return;
            }

            loadActorRequesting = true;

            if (ContextProvider.CurrentWorkingActorContext != null)
            {
                ContextProvider.ActorFactory.Release(ContextProvider.CurrentWorkingActorContext.Actor);
                // ContextProvider.CurrentWorkingActorContext.Reset();
                ContextProvider.CurrentWorkingActorContext = null;
            }

            // SkeletonAnimation skeletonAnimation = null;

            // skeletonAnimation = ActorAssetDatabase.CreateActorAsset(assetLocator).GetComponent<SkeletonAnimation>();
            IAWActor actor = await ContextProvider.ActorFactory.CreateAsync(id, category, cancellationToken: cancellationToken);
            // skeletonAnimation = actor.GameObject.GetComponent<SkeletonAnimation>();
            if (cancellationToken.IsCancellationRequested) return;

            ContextProvider.CurrentWorkingActorContext = new();
            // ContextProvider.CurrentWorkingActorContext.GameObject = skeletonAnimation.gameObject;
            ContextProvider.CurrentWorkingActorContext.Set(actor);

            // UIをリセット
            uiAnimationList.ResetUI(actor);
            uiAnimationControl.ResetUI(loop: uiAnimationControl.IsLoop);
            uiSkinControl.ResetUI(actor);
            uiPlayListControl.ResetUI(actor);

            // Debug.Assert(Form != null);
            // Debug.Assert(Form.GameObject != null);
            // Form.GameObject.GetComponent<UIEntityGroup>().SendEntityEvent(UIEntityEvent.Type.Open);

            loadActorRequesting = false;
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
                uiPlayListControl.AddPlayList(animation);
                return;
            }

            // AnimationStateData stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();

            // TrackEntry cu = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            // if (cu != null && cu.Animation == animation)
            IAWTrack cu = animationController.GetTrack(currentTrackIndex);
            if (cu != null && cu.Animation == animation)
            {
                // 再生チェックマークを更新
                uiAnimationList.SetPlayingCheckmark(sender, currentTrackIndex, false);

                // 同じアニメーションが選択された場合は停止させる
                // animationController.SetEmptyAnimation(currentTrackIndex, stateData.DefaultMix);
                animationController.SetEmptyAnimation(currentTrackIndex);

                // ClearTrack()だけだと一時停止みたいになってしまう
                // skeletonAnimation.AnimationState.ClearTrack(currentTrackIndex);
                uiAnimationControl.TrackAnimationChanged(currentTrackIndex, false);
            }
            else
            {
                // 再生チェックマークを更新
                if (uiAnimationList.SetPlayingCheckmark(sender, currentTrackIndex, true) == false)
                {
                    // 別のトラックが再生中なので再生ができない
                    // 停止させる
                    uiAnimationList.SetPlayingCheckmark(sender, currentTrackIndex, false);
                    // animationController.SetEmptyAnimation(currentTrackIndex, stateData.DefaultMix);
                    animationController.SetEmptyAnimation(currentTrackIndex);
                    uiAnimationControl.TrackAnimationChanged(currentTrackIndex, false);
                    return;
                }

                // if (cu != null && cu.Animation != null)
                // {
                //     var stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();
                //     stateData.SetMix(cu.Animation, animation, stateData.DefaultMix);
                // }

                // TrackEntry trackEntry =
                var track = animationController.SetAnimation(
                    currentTrackIndex, animation: animation, loop: uiAnimationControl.IsLoop);
                // MixDurationを0にしないとDefaultMixが適応されない?
                // trackEntry.MixDuration = 3.0f;

                // skeletonAnimation.AnimationState.AddAnimation(
                //     currentTrackIndex, animation: animation, loop: uiAnimationControl.IsLoop, delay: 0.0f);

                track.TimeScale = trackInfoList[currentTrackIndex].Speed;

                uiAnimationControl.TrackAnimationChanged(currentTrackIndex, true);

            }

        }

        public void OnSkinChanged(int index)
        {
            // var skeletonAnimation = ContextProvider.CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            // var skeleton = skeletonAnimation.Skeleton;
            // var skinList = ContextProvider.CurrentWorkingActorContext.Actor.SkinList;

            // if (index < 0 || skinList.Count <= index)
            // {
            //     Debug.Assert(false);
            //     return;
            // }

            // var skin = skeleton.Data.Skins.Items[index];
            // skeleton.SetSkin(skin);
            // skeleton.SetSlotsToSetupPose();
            ContextProvider.CurrentWorkingActorContext.Actor.SetSkin(index);
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
            if (track.Animation != null)
            {
                animationController.SetAnimation(currentTrackIndex, animation: track.Animation, loop: value);
            }
        }

        // index: UIの0-4のボタンのインデックス
        public void OnActiveTrackChanged(int index)
        {
            // Debug.Log($"OnActiveTrackChanged: index={index}");
            if (ContextProvider.CurrentWorkingActorContext == null) return;

            currentTrackIndex = index;

            // UI変更
            uiAnimationControl.ChangeTrack(
                trackInfoList[currentTrackIndex].Speed, trackInfoList[currentTrackIndex].Mix);

            var uiListView = uiAnimationList.gameObject.Find("UIListView").GetComponent<UIListView>();

            // var skeletonAnimation = ContextProvider.CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            // TrackEntry trackEntry = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            Debug.Assert(ContextProvider != null);
            Debug.Assert(ContextProvider.CurrentWorkingActorContext != null);
            Debug.Assert(ContextProvider.CurrentWorkingActorContext.Actor != null);
            Debug.Assert(ContextProvider.CurrentWorkingActorContext.Actor.AnimationController != null);
            var track = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController.GetTrack(currentTrackIndex);
            if (track == null)
            {
                afl.Service.Input.IInputUIEventModule.SetSelectedGameObject(null);
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

            afl.Service.Input.IInputUIEventModule.SetSelectedGameObject(null);
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
                int category = 0;
                IAssetRepository assetRepository = ContextProvider.AssetRepositories.Get(category);

                var list = assetRepository.ToList();
                for (int i = 0; i < list.Count; ++i)
                {
                    var model = list[i] as AssetModel;
                    // var path = Utility.GetDirectoryPath(model.GetName()).Replace(ActorWorkspaceGUIContextProvider.AssetRootDirectory, "");
                    // var path = Utility.GetDirectoryPath(model.GetName()).Replace(ActorWorkspaceGUIContextProvider.AssetRootDirectory, "");
                    // if (path == ForceLoadAssetAtRunning)

                    // string modelFullPath = Utility.GetDirectoryPath(Utility.PathCombine(ActorWorkspaceFormLogic.AssetRootDirectory, model.GetName()));
                    string modelFullPath = model.AssetLocator;
                    // Utility.GetDirectoryPath(Utility.PathCombine(ActorWorkspaceFormLogic.AssetRootDirectory, model.GetName()));
                    Debug.Log($"modelFullPath: {modelFullPath}");
                    // if (path == modelFullPath)
                    {
                        await LoadActorAsync(model.GetId(), category, cancellationToken: cancellationToken);
                        return;
                    }
                }
            }
        }
#endif
    }
}
