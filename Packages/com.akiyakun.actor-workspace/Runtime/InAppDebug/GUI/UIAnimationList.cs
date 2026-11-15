using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using afl;
using afl.UI;
using afl.UI.v1;

namespace ActorWorkspace.InAppDebug
{
    public class UIAnimationList : UIEntityRootGroup
    {
        [SerializeField] UIListView listView;

        protected override async UniTask<int> InnerInitializeAsync(CancellationToken cancellationToken)
        {
            // イベント登録
            {
                listView.OnClick.AddListener(OnClickFromListView);
            }

            return await UniTask.FromResult<int>(GeneralReturnCode.Succeeded);
        }

        protected override void OnAwake()
        {
            ContextProvider.EventBus.Subscribe(ActorWorkspaceFormEvents.ResetUI, (IAWActor actor) => ResetUI(actor));
        }

        protected override void InnerTerminate()
        {
            // イベント解除
            {
                listView.OnClick.RemoveListener(OnClickFromListView);
            }
        }

        void ResetUI(IAWActor actor)
        {
            ResetListView(actor);
        }

        public void ResetListView(IAWActor actor)
        {
            Debug.Assert(actor != null);
            Debug.Assert(actor.AnimationController != null);

            listView.Clear();

            var animationList = actor.AnimationController.GetAnimationList();
            for (int i = 0; i < animationList.Count; i++)
            {
                var animation = animationList[i];
                var entity = listView.AddEntity();
                entity.Id = i;
                entity.UserData = animation;
                entity.transform.Find("DefaultButton/Text").GetComponent<TMP_Text>().text = animation.Name;
                entity.SetStay();
            }
        }
        /// <returns>既に別のトラックが再生中の場合falseが返ります</returns>
        ///
        /// MEMO:
        /// チェックマークTMP_TextのGameObjectはデフォルトで非アクティブ前提のコードです。
        ///
        public bool SetPlayingCheckmark(UIEntity entity, int trackIndex, bool isPlaying)
        {
            string trackIndexText = trackIndex.ToString();

            // 同じトラック番号で既に再生中の場合はチェックを外す
            var entityList = listView.ContentRoot.GetActiveEntityList();
            for (int i = 0; i < entityList.Count; i++)
            {
                var tmpText = entityList[i].transform.Find("PlayingMark/Text").GetComponent<TMP_Text>();
                if (tmpText.text == trackIndexText)
                {
                    // 古い再生トラックなのでチェックを外す
                    tmpText.gameObject.SetActive(false);
                }
            }

            // 再生中のトラックをチェックする
            if (isPlaying == true)
            {
                var tmpText = entity.transform.Find("PlayingMark/Text").GetComponent<TMP_Text>();

                if (tmpText.gameObject.activeSelf == true)
                {
                    // 他のトラックが再生中なので失敗
                    return false;
                }

                tmpText.text = trackIndexText;
                tmpText.gameObject.SetActive(true);
            }

            return true;
        }

        void OnClickFromListView(GameObject sender)
        {
            var entity = sender.GetComponent<UIEntity>();
            if (entity == null) return;
            OnClickFromEntity(entity);
        }
    }
}
