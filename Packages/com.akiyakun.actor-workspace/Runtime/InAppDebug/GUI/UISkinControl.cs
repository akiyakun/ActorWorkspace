using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.InputSystem;
using afl;
using afl.UI;
using afl.UI.v1;

namespace ActorWorkspace.InAppDebug
{
    public class UISkinControl : UIEntityRootGroup
    {
        [SerializeField] UIListView listView;

        // 選んだスキンの番号(SkinList の番号。合成するときは合成する順)
        public event System.Action<IReadOnlyList<int>> OnSkinsChanged;

        readonly List<string> skinNames = new();
        readonly List<int> selectedSkinIndices = new();
        public IReadOnlyList<int> SelectedSkinIndices => selectedSkinIndices;

        protected override async UniTask<int> InnerInitializeAsync(CancellationToken cancellationToken)
        {
            listView.OnClick.AddListener(OnClickFromListView);
            return await UniTask.FromResult<int>(GeneralReturnCode.Succeeded);
        }

        protected override void OnAwake()
        {
            ContextProvider.EventBus.Subscribe(ActorWorkspaceFormEvents.ResetUI, (IAWActor actor) => ResetUI(actor));
        }

        protected override void InnerTerminate()
        {
            listView.OnClick.RemoveListener(OnClickFromListView);
        }

        void ResetUI(IAWActor actor)
        {
            // SkeletonData skeletonData =  skeletonAnimation.Skeleton.Data;
            // ExposedList<Skin> skins = skeletonData.Skins;

            listView.Clear();
            skinNames.Clear();

            var skinList = actor.SkinList;
            for (int i = 0; i < skinList.Count; i++)
            {
                var entity = listView.AddEntity();
                entity.Id = i;
                entity.name = skinList[i].Name;
                skinNames.Add(skinList[i].Name);
                entity.SetStay();
            }
            ShowSelectedSkins(actor.SkinIndices);
        }

        // 付けているスキンを一覧に表示する。名前の前に、合成する順の番号を付ける
        public void ShowSelectedSkins(IReadOnlyList<int> skinIndices)
        {
            // MEMO: SelectedSkinIndices をそのまま渡されたときは、消すと中身が無くなるので写さない
            if (!ReferenceEquals(skinIndices, selectedSkinIndices))
            {
                selectedSkinIndices.Clear();
                for (int i = 0; i < skinIndices.Count; i++) selectedSkinIndices.Add(skinIndices[i]);
            }

            foreach (var entity in listView.ItemList)
            {
                if (entity.Id < 0 || skinNames.Count <= entity.Id) continue;
                var order = selectedSkinIndices.IndexOf(entity.Id);
                var text = entity.GetComponentInChildren<TMP_Text>();
                text.text = order >= 0 ? $"[{order + 1}] {skinNames[entity.Id]}" : skinNames[entity.Id];
                text.fontStyle = order >= 0 ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        // 一覧に出しているスキンの文字(テスト用)。無ければ null
        public string GetSkinLabel(int skinIndex)
        {
            foreach (var entity in listView.ItemList)
            {
                if (entity.Id == skinIndex) return entity.GetComponentInChildren<TMP_Text>().text;
            }
            return null;
        }

        // スキンを選ぶ。additive が true なら、今のスキンに足して合成する(選んでいるスキンなら外す)
        public void SelectSkin(int skinIndex, bool additive)
        {
            var skinIndices = new List<int>();
            if (additive)
            {
                skinIndices.AddRange(selectedSkinIndices);
                if (skinIndices.Contains(skinIndex))
                {
                    // 最後の 1 つは外さない
                    if (skinIndices.Count <= 1) return;
                    skinIndices.Remove(skinIndex);
                }
                else
                {
                    skinIndices.Add(skinIndex);
                }
            }
            else
            {
                skinIndices.Add(skinIndex);
            }
            OnSkinsChanged?.Invoke(skinIndices);
        }

        void OnClickFromListView(GameObject sender)
        {
            var entity = sender.GetComponent<UIEntity>();
            // Shift を押しながらクリックすると合成する
            var additive = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
            SelectSkin(entity.Id, additive);
        }

    }
}
