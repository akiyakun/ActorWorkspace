#nullable enable
using ActorWorkspace.MasterData;
using afl;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

namespace ActorWorkspace.InAppDebug
{
    // See also: ActorWorkspaceFormView.cs
    // アクタービューワーの一覧表示(Catalog View)
    // カテゴリーのアクターを PageSize 体ずつ読み込み、カメラの写す範囲に格子状に並べる
    //
    // MEMO: 並べたアクターは ActorManager に入らないので、モーションだけここで進める(移動などのビヘイビアーは動かさない)
    public class ActorCatalogView : System.IDisposable
    {
        public const int ColumnCount = 5;
        public const int RowCount = 4;
        public const int PageSize = ColumnCount * RowCount;

        // 画面の上のメニューバーとツールバーに隠れる分として空ける割合
        const float TopMarginRate = 0.1f;
        // マスの下端から足元までの割合。マスの下端に名前を出すので、足元より下にはみ出すアクターでも重なりにくい
        const float FootHeightRate = 0.25f;
        const float LabelFontSize = 20.0f;
        // MEMO: 行の高さより低いと、Ellipsis で文字がすべて消える
        const float LabelHeight = 40.0f;
        const float LabelBottomMargin = 8.0f;

        // 並べているアクター
        public class Entry
        {
            public IAWActor Actor = null!;
            public ActorAssetInfo AssetInfo = null!;
            // ワールド座標でのマス
            public Rect Cell;
            public TMP_Text Label = null!;
        }

        ActorWorkspaceFormContextProvider contextProvider;
        Camera actorCamera;
        // actorCamera の映像が表示されている範囲
        RectTransform actorViewRect;
        TMP_FontAsset labelFont;
        Vector3 cameraHomePosition;

        List<Entry> entries = new();
        public IReadOnlyList<Entry> Entries => entries;

        public int Category { get; private set; }
        public int Page { get; private set; }
        public int PageCount { get; private set; } = 1;
        public bool IsLoading { get; private set; }

        public ActorCatalogView(
            ActorWorkspaceFormContextProvider contextProvider,
            Camera actorCamera,
            RectTransform actorViewRect,
            TMP_FontAsset labelFont
        )
        {
            this.contextProvider = contextProvider;
            this.actorCamera = actorCamera;
            this.actorViewRect = actorViewRect;
            this.labelFont = labelFont;
            cameraHomePosition = actorCamera.transform.position;
        }

        // From IDisposable
        public void Dispose()
        {
            Clear();
        }

        public void DoUpdate(float deltaTime)
        {
            int count = entries.Count;
            for (int i = 0; i < count; ++i)
            {
                var entry = entries[i];
                entry.Actor.AnimationController.DoUpdate(deltaTime);
                UpdateLabelPosition(entry);
            }
        }

        // カテゴリーの指定のページを読み込む。今並べているアクターは解放する
        // 読み込めないアクターは、警告を出してそのマスを空ける
        public async UniTask<int> LoadPageAsync(int category, int page, CancellationToken cancellationToken)
        {
            if (IsLoading) return GeneralReturnCode.Failed;

            IsLoading = true;
            try
            {
                Clear();

                var assetList = contextProvider.GetAssetList(category);
                Category = category;
                PageCount = Mathf.Max((assetList.Count + PageSize - 1) / PageSize, 1);
                Page = Mathf.Clamp(page, 0, PageCount - 1);

                int firstIndex = Page * PageSize;
                int count = Mathf.Min(PageSize, assetList.Count - firstIndex);
                for (int i = 0; i < count; ++i)
                {
                    var assetInfo = assetList[firstIndex + i];
                    var actor = await CreateActorAsync(assetInfo, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        if (actor != null) contextProvider.ActorFactory.Release(actor);
                        return GeneralReturnCode.Canceled;
                    }
                    if (actor == null) continue;

                    AddEntry(actor, assetInfo, i);
                }

                return GeneralReturnCode.Succeeded;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // 並べているアクターをすべて解放する
        public void Clear()
        {
            // MEMO: 解放の途中で例外が出ても同じアクターを 2 回解放しないよう、先にリストから外す
            for (int i = entries.Count - 1; i >= 0; --i)
            {
                var entry = entries[i];
                entries.RemoveAt(i);
                // MEMO: フォームと一緒に破棄されるときは、もう無いことがある
                if (entry.Label != null) GameObject.Destroy(entry.Label.gameObject);
                contextProvider.ActorFactory.Release(entry.Actor);
            }
        }

        // カメラのビューポート座標のマスにいるアクターを返す
        public Entry? FindEntry(Vector2 viewportPoint)
        {
            Vector2 worldPoint = actorCamera.ViewportToWorldPoint(viewportPoint);

            int count = entries.Count;
            for (int i = 0; i < count; ++i)
            {
                if (entries[i].Cell.Contains(worldPoint)) return entries[i];
            }
            return null;
        }

        async UniTask<IAWActor?> CreateActorAsync(ActorAssetInfo assetInfo, CancellationToken cancellationToken)
        {
            var param = new ActorCreateParam(assetInfo.AssetModel.Id, assetInfo.Category);
            IAWActor? actor = null;
            try
            {
                actor = await contextProvider.ActorFactory.CreateAsync(param, cancellationToken);
                if (actor == null || cancellationToken.IsCancellationRequested) return actor;

                ActorWorkspaceFormView.SetupActor(actor);
                PlayMotion(actor);
                return actor;
            }
            catch (System.OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return actor;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"ActorCatalogView: CreateActorAsync: Failed to load actor. Id={param.Id}, Category={param.Category}. {exception}");
                if (actor != null) contextProvider.ActorFactory.Release(actor);
                return null;
            }
        }

        void PlayMotion(IAWActor actor)
        {
            var animationController = actor.AnimationController;
            var animationList = animationController.GetAnimationList();
            if (animationList.Count == 0) return;

            var animation = animationList[0];
            int count = animationList.Count;
            for (int i = 0; i < count; ++i)
            {
                if (animationList[i].Name != contextProvider.CatalogMotionName) continue;

                animation = animationList[i];
                break;
            }

            var option = new AWAnimationOption();
            option.Flags = (uint)AWAnimationOptionFlag.Loop | (uint)AWAnimationOptionFlag.Immediate;
            animationController.UseRootMotion = false;
            animationController.SetAnimation(animation.Name, option);
        }

        void AddEntry(IAWActor actor, ActorAssetInfo assetInfo, int cellIndex)
        {
            var entry = new Entry();
            entry.Actor = actor;
            entry.AssetInfo = assetInfo;
            entry.Cell = GetCell(cellIndex);
            // MEMO: 名前が空のアクターは、ActorAssetInfo.Name に ID とアセットの場所が入っている
            string actorName = assetInfo.AssetModel.GetName();
            entry.Label = CreateLabel(string.IsNullOrEmpty(actorName) ? assetInfo.Name : $"{assetInfo.AssetModel.Id}: {actorName}");

            var position = actor.GameObject.transform.position;
            actor.GameObject.transform.position = new Vector3(
                entry.Cell.center.x,
                entry.Cell.yMin + entry.Cell.height * FootHeightRate,
                position.z
            );

            UpdateLabelPosition(entry);
            entries.Add(entry);
        }

        // カメラの写す範囲を、上から順に割ったマス
        Rect GetCell(int cellIndex)
        {
            float viewHeight = actorCamera.orthographicSize * 2.0f;
            float viewWidth = viewHeight * actorCamera.aspect;
            float cellWidth = viewWidth / ColumnCount;
            float cellHeight = viewHeight * (1.0f - TopMarginRate) / RowCount;

            int column = cellIndex % ColumnCount;
            int row = cellIndex / ColumnCount;
            return new Rect(
                cameraHomePosition.x - (viewWidth * 0.5f) + (cellWidth * column),
                cameraHomePosition.y + (viewHeight * (0.5f - TopMarginRate)) - (cellHeight * (row + 1)),
                cellWidth,
                cellHeight
            );
        }

        TMP_Text CreateLabel(string text)
        {
            var obj = new GameObject("CatalogLabel", typeof(RectTransform));
            obj.layer = actorViewRect.gameObject.layer;

            var rectTransform = (RectTransform)obj.transform;
            rectTransform.SetParent(actorViewRect, worldPositionStays: false);
            rectTransform.pivot = new Vector2(0.5f, 0.0f);
            rectTransform.sizeDelta = new Vector2(actorViewRect.rect.width / ColumnCount, LabelHeight);

            var label = obj.AddComponent<TextMeshProUGUI>();
            label.font = labelFont;
            label.fontSize = LabelFontSize;
            label.alignment = TextAlignmentOptions.Bottom;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.raycastTarget = false;
            label.text = text;
            return label;
        }

        // カメラを動かしてもマスの下端に出るように、ビューポート座標でアンカーを合わせる
        void UpdateLabelPosition(Entry entry)
        {
            var cellBottom = new Vector2(entry.Cell.center.x, entry.Cell.yMin);
            Vector2 viewportPoint = actorCamera.WorldToViewportPoint(cellBottom);
            var rectTransform = entry.Label.rectTransform;
            rectTransform.anchorMin = viewportPoint;
            rectTransform.anchorMax = viewportPoint;
            rectTransform.anchoredPosition = new Vector2(0.0f, LabelBottomMargin);
        }
    }
}
#nullable restore
