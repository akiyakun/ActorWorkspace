using ActorWorkspace.MasterData;
using afl;
using afl.Service.Effects;
using afl.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
        Button loadAssetButton;
        GameObject openAssetDialog;
        Vector2 assetListScrollPosition = Vector2.zero;
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

        // アクターの輪郭が見やすいよう、黒ではなく灰色にする
        static readonly Color ActorBackgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

        public enum ViewMode
        {
            Single = 0,
            Catalog,
        }
        public ViewMode CurrentViewMode { get; private set; } = ViewMode.Single;

        public ActorCatalogView CatalogView { get; private set; }

        Vector3 cameraHomePosition;
        // actorCamera の映像が表示されている範囲
        RectTransform actorViewRect;
        // Single View に戻ったときに読み込むアクター
        ActorCreateParam? lastActorParam;
        // Single View でだけ使うパネル
        List<GameObject> singleViewPanels = new();

        UIMenuBar.Item openMenuItem;
        UIMenuBar.Item flipMenuItem;
        UIMenuBar.Item singleViewMenuItem;
        UIMenuBar.Item catalogViewMenuItem;

        List<Button> singleViewToolButtons = new();
        List<Button> catalogToolButtons = new();
        List<Button> categoryToolButtons = new();
        Button previousPageToolButton;
        TMP_Text pageLabel;
        Button nextPageToolButton;

        // 読み込みの途中は、表示を切り替えられない
        public bool IsBusy => loadActorRequesting || CatalogView.IsLoading;

        // Single View で表示しているアクター
        public IAWActor WorkingActor => ContextProvider.CurrentWorkingActorContext.Actor;

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
                eventBag.In(openAssetDialog.GetComponent<UIEntityRootGroup>(),
                    (entity) => entity.OnStay += OnOpenAssetDialogStay,
                    (entity) => entity.OnStay -= OnOpenAssetDialogStay);

                OnSetupMenu(
                    Form.Root.transform.Find("UIMenuBar").GetComponent<UIMenuBar>(),
                    Form.Root.transform.Find("UIToolBar").GetComponent<UIToolBar>()
                );

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
                ActorCamera.backgroundColor = ActorBackgroundColor;

                var cameraControlArea = Form.Root.transform.Find("CameraControlArea").GetComponent<CameraControlArea>();
                cameraControlArea.SetSourceCamera(ActorCamera);

                // 一覧のアクターのクリック
                // MEMO: ドラッグしたときはカメラの移動なので、クリックにはならない
                var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                cameraControlArea.gameObject.AddComponent<EventTrigger>().triggers.Add(clickEntry);
                eventBag.In(clickEntry.callback,
                    (entity) => entity.AddListener(OnClickActorView),
                    (entity) => entity.RemoveListener(OnClickActorView));
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

            // 一覧表示
            {
                singleViewPanels.Add(uiAnimationList.gameObject);
                singleViewPanels.Add(uiSkinControl.gameObject);
                singleViewPanels.Add(uiAnimationControl.gameObject);
                singleViewPanels.Add(uiPlayListControl.gameObject);

                cameraHomePosition = ActorCamera.transform.position;
                actorViewRect = FindActorViewRect();
                CatalogView = new ActorCatalogView(ContextProvider, ActorCamera, actorViewRect, pageLabel.font);

                UpdateViewModeUI();
            }

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
            var actor = ContextProvider.CurrentWorkingActorContext.Actor;
            if (actor != null)
            {
                ContextProvider.CurrentWorkingActorContext.Reset();
                ContextProvider.ActorFactory.Release(actor);
            }

            CatalogView.Dispose();
            eventBag.Dispose();
            uiAnimationList.OnClickEntity -= OnClickEntityFromAnimationList;
            uiSkinControl.OnSkinChanged -= OnSkinChanged;
            uiAnimationControl.OnSpeedValueChanged -= OnSpeedValueChanged;
            uiAnimationControl.OnMixValueChanged -= OnMixValueChanged;
            uiAnimationControl.OnLoopValueChanged -= OnLoopValueChanged;
            uiAnimationControl.OnActiveTrackChanged -= OnActiveTrackChanged;
            uiAnimationControl.OnRootMotionValueChanged -= OnRootMotionValueChanged;
        }

        // メニューバーとツールバーの中身を作る
        // ゲーム側のメニューを足したいときは、派生クラスで override する
        // MEMO: 一覧表示の初期化で使うので、override でも base.OnSetupMenu を呼ぶこと
        protected virtual void OnSetupMenu(UIMenuBar menuBar, UIToolBar toolBar)
        {
            openMenuItem = menuBar.AddMenu("File").AddItem("Open...", OnOpenAsset);
            flipMenuItem = menuBar.AddMenu("View").AddItem("Flip", OnFilpActor);

            var modeMenu = menuBar.AddMenu("Mode");
            singleViewMenuItem = modeMenu.AddItem("Single View", OnSingleView);
            catalogViewMenuItem = modeMenu.AddItem("Catalog View", OnCatalogView);

            singleViewToolButtons.Add(toolBar.AddButton("Open", OnOpenAsset));
            singleViewToolButtons.Add(toolBar.AddButton("Flip", OnFilpActor));

            for (int i = 0; i < ContextProvider.AssetRepositories.Count; ++i)
            {
                int category = i;
                var button = toolBar.AddButton(ContextProvider.GetCategoryName(category), () => OnClickCategory(category));
                categoryToolButtons.Add(button);
                catalogToolButtons.Add(button);
            }

            previousPageToolButton = toolBar.AddButton("<", () => OnClickPage(-1));
            // ページの表示。押せないボタンで出す
            var pageToolButton = toolBar.AddButton(string.Empty, () => { });
            pageToolButton.interactable = false;
            pageLabel = pageToolButton.GetComponentInChildren<TMP_Text>(includeInactive: true);
            nextPageToolButton = toolBar.AddButton(">", () => OnClickPage(+1));
            catalogToolButtons.Add(previousPageToolButton);
            catalogToolButtons.Add(pageToolButton);
            catalogToolButtons.Add(nextPageToolButton);
        }

        // From UIFormViewBase
        // MEMO: 表示中のアクターは ActorManager に入らないので、モーションだけここで進める(移動などのビヘイビアーは動かさない)
        public override void DoUpdate(float deltaTime)
        {
            if (CurrentViewMode == ViewMode.Catalog)
            {
                CatalogView.DoUpdate(deltaTime);
                return;
            }

            var actor = ContextProvider.CurrentWorkingActorContext.Actor;
            if (actor == null) return;

            actor.AnimationController.DoUpdate(deltaTime * playbackSpeed);
        }

        // 全てのアセットのリストを生成して返します
        public List<ActorAssetInfo> GetAllAssetList()
        {
            List<ActorAssetInfo> list = new();

            for (int categoty = 0; categoty < ContextProvider.AssetRepositories.Count; ++categoty)
            {
                list.AddRange(ContextProvider.GetAssetList(categoty));
            }

            return list;
        }

        // Single View にする。param を省略すると、最後に見ていたアクターを読み込む
        public async UniTask<int> ShowSingleViewAsync(ActorCreateParam? param, CancellationToken cancellationToken)
        {
            if (IsBusy) return GeneralReturnCode.Failed;

            CatalogView.Clear();
            ActorCamera.transform.position = cameraHomePosition;
            SetViewMode(ViewMode.Single);

            var actorParam = param ?? lastActorParam;
            if (actorParam == null) return GeneralReturnCode.Succeeded;
            return await LoadActorAsync(actorParam.Value, cancellationToken);
        }

        // Catalog View にして、最後に見ていたページを読み込む
        public async UniTask<int> ShowCatalogViewAsync(CancellationToken cancellationToken)
        {
            if (IsBusy) return GeneralReturnCode.Failed;
            if (CurrentViewMode == ViewMode.Catalog) return GeneralReturnCode.Succeeded;

            if (openAssetDialog.activeSelf) CloseOpenAssetDialog();

            ReleaseWorkingActor();
            ActorCamera.transform.position = cameraHomePosition;
            SetViewMode(ViewMode.Catalog);

            return await ShowCatalogPageAsync(CatalogView.Category, CatalogView.Page, cancellationToken);
        }

        public async UniTask<int> ShowCatalogPageAsync(int category, int page, CancellationToken cancellationToken)
        {
            if (IsBusy || CurrentViewMode != ViewMode.Catalog) return GeneralReturnCode.Failed;

            var task = CatalogView.LoadPageAsync(category, page, cancellationToken);
            UpdateViewModeUI();

            try
            {
                return await task;
            }
            finally
            {
                // MEMO: キャンセルのときは、フォームが破棄されていてボタンがもう無いことがある
                if (cancellationToken.IsCancellationRequested == false) UpdateViewModeUI();
            }
        }

        void SetViewMode(ViewMode viewMode)
        {
            CurrentViewMode = viewMode;

            bool isSingleView = viewMode == ViewMode.Single;
            foreach (var panel in singleViewPanels)
            {
                panel.SetActive(isSingleView);
            }

            UpdateViewModeUI();
        }

        // メニューとツールバーを今の状態に合わせる
        void UpdateViewModeUI()
        {
            bool isSingleView = CurrentViewMode == ViewMode.Single;

            openMenuItem.Interactable = isSingleView;
            flipMenuItem.Interactable = isSingleView;
            singleViewMenuItem.Interactable = isSingleView == false;
            catalogViewMenuItem.Interactable = isSingleView;

            foreach (var button in singleViewToolButtons)
            {
                button.gameObject.SetActive(isSingleView);
            }
            foreach (var button in catalogToolButtons)
            {
                button.gameObject.SetActive(isSingleView == false);
            }

            bool isLoading = CatalogView.IsLoading;
            for (int i = 0; i < categoryToolButtons.Count; ++i)
            {
                categoryToolButtons[i].interactable = isLoading == false && i != CatalogView.Category;
            }
            previousPageToolButton.interactable = isLoading == false && CatalogView.Page > 0;
            nextPageToolButton.interactable = isLoading == false && CatalogView.Page < CatalogView.PageCount - 1;
            pageLabel.text = $"{CatalogView.Page + 1} / {CatalogView.PageCount}";
        }

        // カメラの映像を RawImage に出しているときはその範囲、それ以外は画面全体を返す
        RectTransform FindActorViewRect()
        {
            var targetTexture = ActorCamera.targetTexture;
            if (targetTexture != null)
            {
                foreach (var rawImage in Form.Root.GetComponentsInChildren<RawImage>(includeInactive: true))
                {
                    if (rawImage.texture == targetTexture) return rawImage.rectTransform;
                }
            }
            return (RectTransform)Form.Root.transform;
        }

        void ReleaseWorkingActor()
        {
            var actor = ContextProvider.CurrentWorkingActorContext.Actor;
            if (actor == null) return;

            ContextProvider.CurrentWorkingActorContext.Reset();
            ReleaseActor(actor);
        }

        void ReleaseActor(IAWActor actor)
        {
            actor.ElementActive = false;
            actor.GameObject.SetActive(false);
            OnRemoveActor?.Invoke(actor);
            ContextProvider.ActorFactory.Release(actor);
        }

        void OnSingleView()
        {
            ShowSingleViewAsync(null, ((UIForm)Form).destroyCancellationToken).Forget();
        }

        void OnCatalogView()
        {
            ShowCatalogViewAsync(((UIForm)Form).destroyCancellationToken).Forget();
        }

        void OnClickCategory(int category)
        {
            ShowCatalogPageAsync(category, 0, ((UIForm)Form).destroyCancellationToken).Forget();
        }

        void OnClickPage(int offset)
        {
            ShowCatalogPageAsync(CatalogView.Category, CatalogView.Page + offset, ((UIForm)Form).destroyCancellationToken).Forget();
        }

        // 一覧のアクターをクリックしたら、そのアクターの Single View にする
        void OnClickActorView(BaseEventData baseEventData)
        {
            if (CurrentViewMode != ViewMode.Catalog) return;

            var eventData = (PointerEventData)baseEventData;
            if (eventData.button != PointerEventData.InputButton.Left || eventData.dragging) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                actorViewRect, eventData.position, eventData.pressEventCamera, out var localPoint) == false) return;
            if (actorViewRect.rect.Contains(localPoint) == false) return;

            var entry = CatalogView.FindEntry(Rect.PointToNormalized(actorViewRect.rect, localPoint));
            if (entry == null) return;

            var param = new ActorCreateParam(entry.AssetInfo.AssetModel.Id, entry.AssetInfo.Category);
            ShowSingleViewAsync(param, ((UIForm)Form).destroyCancellationToken).Forget();
        }

        void OnOpenAsset()
        {
            if (CurrentViewMode != ViewMode.Single || openAssetDialog.activeSelf) return;

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
        }

        // MEMO: 開くアニメーションの途中は ScrollRect が位置を正しく計算できずにずれるので、開き終わってから戻す
        void OnOpenAssetDialogStay()
        {
            assetListScrollRect.StopMovement();
            assetListScrollRect.content.anchoredPosition = assetListScrollPosition;
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

            CloseOpenAssetDialog();
        }

        void CloseOpenAssetDialog()
        {
            assetListScrollRect.StopMovement();
            var group = openAssetDialog.GetComponent<UIEntityRootGroup>();
            // MEMO: 開くアニメーションの途中はずれた位置なので保存しない
            if (group.CurrentState == UIEntityState.Stay) assetListScrollPosition = assetListScrollRect.content.anchoredPosition;
            group.SetCloseComplateAtInactive(true);
            group.SendEntityEvent(UIEntityEvent.Type.Close);
        }

        // ビューワーで動かすアクターの準備
        internal static void SetupActor(IAWActor actor)
        {
            actor.GameObject.SetActive(true);
            var monoBehaviour = actor.ActorDisplay.GetComponentsInChildren<MonoBehaviour>();
            foreach (var mb in monoBehaviour)
            {
                if (mb.GetType().FullName == "BehaviorDesigner.Runtime.BehaviorTree")
                {
                    mb.enabled = false;
                }
            }
        }

        public async UniTask<int> LoadActorAsync(ActorCreateParam param, CancellationToken cancellationToken)
        {
            // MEMO: Catalog View にした直後は、ダイアログが閉じ終わるまで Load ボタンを押せる
            if (loadActorRequesting || CurrentViewMode != ViewMode.Single) return GeneralReturnCode.Failed;
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

                SetupActor(actor);
                ContextProvider.CurrentWorkingActorContext.Set(actor);
                currentTrackIndex = 0;
                ContextProvider.EventBus.Publish(ActorWorkspaceFormEvents.ResetUI, actor);
                OnCreatedActor?.Invoke(actor);
                committed = true;
                lastActorParam = param;

                if (previousActor != null) ReleaseActor(previousActor);

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

                // MEMO: 読み込みの後に残ったトリガーがあると、切り替えた直後に Animator がそのステートへ戻ってしまう
                animationController.AnimationParameter.AllResetTrigger();

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
