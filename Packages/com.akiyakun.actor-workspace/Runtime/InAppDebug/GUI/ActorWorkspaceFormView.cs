using ActorWorkspace.MasterData;
using afl;
using afl.MasterData;
using afl.Service.Effects;
using afl.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceFormView : UIFormViewBase<ActorWorkspaceFormContextProvider>
    {
        public const string AssetRootDirectory = "Assets/AssetBundleData/Actor";
        public const string AnimationListControlPath = "Root/AnimationListControl";

        // [SerializeField] Camera actorCamera;
        public Camera ActorCamera { get; private set; } = null!;
        // [SerializeField] Camera uiCamera;
        // public Camera UICamera => uiCamera;

        // public Camera CurrentCamera => navigationCamera;

        public event System.Action<IAWActor> OnCreatedActor;
        public event System.Action<IAWActor> OnRemoveActor;
        public event System.Action<float> OnSpeedChanged;

        UIAnimationList uiAnimationList;
        UISkinControl uiSkinControl;
        UIAnimationControl uiAnimationControl;
        UIPlayListControl uiPlayListControl;
        UIListView assetListView;
        ScrollRect assetListScrollRect;
        Button openButton;
        Button flipButton;
        Button loadAssetButton;
        GameObject openAssetDialog;
        Vector2 assetListScrollPosition = new Vector2(0.0f, 1.0f);
        bool assetListCreated;
        float playbackSpeed = 1.0f;

        EventBag eventBag = new();
        bool loadActorRequesting;
        int currentTrackIndex = 0;

        class TrackInfo
        {
            public float Mix = 0.25f;
        }
        List<TrackInfo> trackInfoList = ListEx.Create<TrackInfo>(16);

        // From IAsyncInitializable
        // public bool IsInitialized { get; protected set; }

        // void Awake()
        // {
        //     // openAssetDialog.SetActive(false);
        // }

        // MEMO: 設定のしかた何か違う気がする、Context作ってそれを内部でGetContextしたほうが？
        ActorWorkspaceFormContextProvider injectContextProvider;
        public void SetContextProvider(ActorWorkspaceFormContextProvider contextProvider)
        {
            injectContextProvider = contextProvider;
        }

        // From UIFormViewBase
        protected override ActorWorkspaceFormContextProvider CreateContextProvider()
        {
            Debug.Assert(injectContextProvider != null);
            return injectContextProvider;
        }

        // From IAsyncInitializable
        // public async UniTask<int> InitializeAsync(CancellationToken cancellationToken = default)
        // From UIFormLogic
        protected override async UniTask<int> InnerInitializeAsync(CancellationToken cancellationToken)
        {
            // ContextProvider = contextProvider;
            Debug.Assert(ContextProvider != null, "先に SetContextProvider() を呼び出してください。");

            // VariableTable
            {
                ActorCamera = Form.VariableTable.Get("ActorCamera").GetComponent<Camera>();
                openAssetDialog = Form.VariableTable.Get("OpenAssetDialog").GetGameObject();
                // 遅れて実行される Awake が初回の Open を打ち消さないよう、表示はこの View で管理する。
                var activator = openAssetDialog.GetComponent<ObjectActivator>();
                if (activator != null) activator.enabled = false;
                openAssetDialog.SetActive(false);
                assetListView = openAssetDialog.GetComponent<UIListView>("UIListView");
                assetListScrollRect = assetListView.GetComponentInChildren<ScrollRect>(includeInactive: true);

                openButton = Form.VariableTable.Get("OpenButton").GetComponent<Button>();
                eventBag.In(openButton,
                    (entity) => entity.onClick.AddListener(OnOpenAsset),
                    (entity) => entity.onClick.RemoveListener(OnOpenAsset));

                flipButton = Form.VariableTable.Get("FlipButton").GetComponent<Button>();
                eventBag.In(flipButton,
                    (entity) => entity.onClick.AddListener(OnFilpActor),
                    (entity) => entity.onClick.RemoveListener(OnFilpActor));

                loadAssetButton = Form.VariableTable.Get("LoadAssetButton").GetComponent<Button>();
                eventBag.In(loadAssetButton,
                    (entity) => entity.onClick.AddListener(OnLoadAsset),
                    (entity) => entity.onClick.RemoveListener(OnLoadAsset));
            }

            // アクターカメラの設定
            {
                // var originCamera = transform.Find("Root/ActorCamera").GetComponent<Camera>();
                // if (ActorCamera != originCamera)
                // {
                //     // 別のカメラが設定されているときオリジナルは非表示にする
                //     originCamera.gameObject.SetActive(false);
                // }

                // MEMO: 250810 何か変更して刺激を与えないとカメラ描画を始めてくれない(???)
                ActorCamera.enabled = false;
                ActorCamera.enabled = true;

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
                uiAnimationControl.OnRootMotionValueChanged += OnRootMotionValueChanged;
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

            // Debug.Log("ActorWorkspaceGUI initialized successfully.");

            // IsInitialized = true;
            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        // From IAsyncInitializable
        // public void Terminate()
        // {
        // }
        // From UIFormLogic
        protected override void InnerTerminate()
        {
            eventBag.Dispose();
            uiAnimationList.OnClickEntity -= OnClickEntityFromAnimationList;
            uiSkinControl.OnSkinChanged -= OnSkinChanged;
            uiAnimationControl.OnSpeedValueChanged -= OnSpeedValueChanged;
            uiAnimationControl.OnMixValueChanged -= OnMixValueChanged;
            uiAnimationControl.OnLoopValueChanged -= OnLoopValueChanged;
            uiAnimationControl.OnActiveTrackChanged -= OnActiveTrackChanged;
            uiAnimationControl.OnRootMotionValueChanged -= OnRootMotionValueChanged;
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
                        info.Name = $"{model.Id:0000}: {info.AssetModel.AssetLocator}";
                    }

                    list.Add(info);
                }
            }

            return list;
        }

        void OnOpenAsset()
        {
            if (openAssetDialog.activeSelf) return;

            // 一覧を再生成せず、選択状態とスクロール位置を保持する。
            if (assetListCreated == false)
            {
                assetListView.Clear();
                foreach (var info in GetAllAssetList())
                {
                    var entity = assetListView.AddEntity();
                    entity.UserData = info;
                    entity.name = info.Name;
                    entity.GetComponentInChildren<TMP_Text>().text = info.Name;
                    entity.SetStay();
                }
                assetListCreated = true;
            }

            openAssetDialog.GetComponent<UIEntityRootGroup>().SendEntityEvent(UIEntityEvent.Type.Open);
            Canvas.ForceUpdateCanvases();
            assetListScrollRect.StopMovement();
            assetListScrollRect.normalizedPosition = assetListScrollPosition;
        }

        void OnFilpActor()
        {
            if (ContextProvider.CurrentWorkingActorContext.Actor == null) return;

            var actor = ContextProvider.CurrentWorkingActorContext.Actor;
            // 左を向いていたら
            if (actor.ActorDisplay.transform.localScale.x < 0.0f)
            {
                // 右を向かせる
                actor.ActorDisplay.SetScaleSign(ValueSignType.PositiveX);
            }
            // 左
            else if (actor.ActorDisplay.transform.localScale.x > 0.0f)
            {
                // 左を向かせる
                actor.ActorDisplay.SetScaleSign(ValueSignType.NegativeX);
            }
        }

        void OnLoadAsset()
        {
            if (loadActorRequesting || assetListView.SelectedEntity == null) return;
            if (assetListView.SelectedEntity.UserData is not ActorAssetInfo assetInfo) return;

            LoadActorRequest(assetInfo.AssetModel.Id, assetInfo.Category);
        }

        public void LoadActorRequest(int id, int category)
        {
            if (loadActorRequesting) return;
            LoadSelectedActorAsync(new ActorCreateParam(id, category, Vector3.zero), ((UIForm)Form).destroyCancellationToken).Forget();
        }

        async UniTask LoadSelectedActorAsync(ActorCreateParam param, CancellationToken cancellationToken)
        {
            int ret = await LoadActorAsync(param, cancellationToken);
            if (ret < 0 || cancellationToken.IsCancellationRequested || openAssetDialog.activeSelf == false) return;

            assetListScrollRect.StopMovement();
            assetListScrollPosition = assetListScrollRect.normalizedPosition;
            var group = openAssetDialog.GetComponent<UIEntityRootGroup>();
            group.SetCloseComplateAtInactive(true);
            group.SendEntityEvent(UIEntityEvent.Type.Close);
        }

        public async UniTask<int> LoadActorAsync(ActorCreateParam param, CancellationToken cancellationToken)
        {
            if (loadActorRequesting) return GeneralReturnCode.Failed;
            if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Canceled;

            loadActorRequesting = true;
            loadAssetButton.interactable = false;
            var previousActor = ContextProvider.CurrentWorkingActorContext.Actor;
            IAWActor actor = null;
            bool committed = false;

            try
            {
                // 読込と UI の準備が成功するまで前のアクターを表示しておく。
                actor = await ContextProvider.ActorFactory.CreateAsync(param, cancellationToken: cancellationToken);
                if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Canceled;
                if (actor == null) return GeneralReturnCode.Failed;

                actor.GameObject.SetActive(true);
                var monoBehaviour = actor.ActorDisplay.GetComponentsInChildren<MonoBehaviour>();
                foreach (var mb in monoBehaviour)
                {
                    if (mb.GetType().FullName == "BehaviorDesigner.Runtime.BehaviorTree")
                    {
                        mb.enabled = false;
                    }
                }
                ContextProvider.CurrentWorkingActorContext.Set(actor);
                currentTrackIndex = 0;
                ContextProvider.EventBus.Publish(ActorWorkspaceFormEvents.ResetUI, actor);
                OnCreatedActor?.Invoke(actor);
                committed = true;

                if (previousActor != null)
                {
                    previousActor.ElementActive = false;
                    previousActor.GameObject.SetActive(false);
                    OnRemoveActor?.Invoke(previousActor);
                    ContextProvider.ActorFactory.Release(previousActor);
                }

                return GeneralReturnCode.Succeeded;
            }
            catch (System.OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return GeneralReturnCode.Canceled;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"ActorWorkspace: Failed to load actor. {exception}");
                return GeneralReturnCode.Failed;
            }
            finally
            {
                try
                {
                    if (committed == false)
                    {
                        ContextProvider.CurrentWorkingActorContext.Actor = previousActor;
                        if (actor != null)
                        {
                            actor.ElementActive = false;
                            actor.GameObject.SetActive(false);
                            ContextProvider.ActorFactory.Release(actor);
                        }
                    }
                }
                finally
                {
                    loadActorRequesting = false;
                    if (loadAssetButton != null) 
                    {
                        loadAssetButton.interactable = true;
                    }
                }
            }
        }


        // アニメーションリストからアニメーションをクリックしたときの処理
        public void OnClickEntityFromAnimationList(UIEntity sender)
        {
            Debug.Log("UIAnimationList: OnClickEntityFromAnimationList");

            // var animation = sender.UserData as Spine.Animation;
            // var skeletonAnimation = ContextProvider.CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            var animation = sender.UserData as IAWAnimation;
            var animationController = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController;

            // Ctrlが押されている場合は再生リストに追加する
            if (Keyboard.current != null && Keyboard.current.ctrlKey.isPressed)
            {
                // uiPlayListControl.AddPlayList(animation);
                ContextProvider.EventBus.Publish(ActorWorkspaceFormEvents.PlayList_Add, animation);
                return;
            }

            // AnimationStateData stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();

            // TrackEntry cu = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            // if (cu != null && cu.Animation == animation)
            IAWTrack cu = animationController.GetTrack(currentTrackIndex);
            // if (cu != null && cu.Animation == animation)
            // {
            //     // 再生チェックマークを更新
            //     uiAnimationList.SetPlayingCheckmark(sender, currentTrackIndex, false);

            //     // 同じアニメーションが選択された場合は停止させる
            //     // animationController.SetEmptyAnimation(currentTrackIndex, stateData.DefaultMix);
            //     animationController.SetEmptyAnimation(currentTrackIndex);

            //     // ClearTrack()だけだと一時停止みたいになってしまう
            //     // skeletonAnimation.AnimationState.ClearTrack(currentTrackIndex);
            //     uiAnimationControl.TrackAnimationChanged(currentTrackIndex, false);
            // }
            // else
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
                var opt = new AWAnimationOption();
                opt.Track = currentTrackIndex;
                opt.Flags = (uint)AWAnimationOptionFlag.Loop | (uint)AWAnimationOptionFlag.Immediate;

                animationController.UseRootMotion = false;

                var track = animationController.SetAnimation(Utility.StringToHashId(animation.Name), opt);
                // MixDurationを0にしないとDefaultMixが適応されない?
                // trackEntry.MixDuration = 3.0f;

                // skeletonAnimation.AnimationState.AddAnimation(
                //     currentTrackIndex, animation: animation, loop: uiAnimationControl.IsLoop, delay: 0.0f);

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
            playbackSpeed = Mathf.Clamp(value, 0.1f, 2.0f);
            OnSpeedChanged?.Invoke(playbackSpeed);
        }

        public void OnMixValueChanged(float value)
        {
            trackInfoList[currentTrackIndex].Mix = value;
            if (ContextProvider.CurrentWorkingActorContext.IsValid == false) return;
            // var skeletonAnimation = ContextProvider.CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            var track = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController.GetTrack(currentTrackIndex);
            if (track == null) return;
            // TrackごとにMixを設定するようなことはできない
            // AnimationStateData stateData = skeletonAnimation.skeletonDataAsset.GetAnimationStateData();
            // stateData.DefaultMix = value;
            track.MixDuration = value;
            trackInfoList[currentTrackIndex].Mix = value;
            // Debug.Log($"OnMixValueChanged: stateData.DefaultMix={stateData.DefaultMix}, value={value}");
        }

        public void OnLoopValueChanged(bool value)
        {
            if (ContextProvider.CurrentWorkingActorContext.IsValid == false) return;
            // var skeletonAnimation = ContextProvider.CurrentWorkingActorContext.GameObject.GetComponent<SkeletonAnimation>();
            // TrackEntry trackEntry = skeletonAnimation.AnimationState.GetCurrent(currentTrackIndex);
            // if (trackEntry == null) return;
            var animationController = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController;
            var track = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController.GetTrack(currentTrackIndex);
            if (track != null && track.Animation != null)
            {
                animationController.SetAnimation(Utility.StringToHashId(track.Animation.Name), value, currentTrackIndex);
            }
        }

        // index: UIの0-4のボタンのインデックス
        public void OnActiveTrackChanged(int index)
        {
            // Debug.Log($"OnActiveTrackChanged: index={index}");
            if (ContextProvider.CurrentWorkingActorContext.IsValid == false) return;

            currentTrackIndex = index;

            // UI変更
            uiAnimationControl.ChangeTrack(
                playbackSpeed, trackInfoList[currentTrackIndex].Mix);

            var uiListView = uiAnimationList.gameObject.GetComponent<UIListView>("UIListView");

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
            var animationList = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController.GetAnimationList();
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

        public void OnRootMotionValueChanged(bool value)
        {
            Debug.Log($"OnRootMotionValueChanged: value={value}");
            var animationController = ContextProvider.CurrentWorkingActorContext.Actor.AnimationController;
            animationController.UseRootMotion = value;
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
            var path = Utility.GetDirectoryPathNormalization(ForceLoadAssetAtRunning);
            Debug.Log($"ForceLoadAssetAtRunning: {path}");

            // if (string.IsNullOrEmpty(ForceLoadAssetAtRunning) == false)
            /* とりあえず何か読み込みしたいときのコード
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
            //*/

            await UniTask.CompletedTask;
        }
#endif
    }
}
