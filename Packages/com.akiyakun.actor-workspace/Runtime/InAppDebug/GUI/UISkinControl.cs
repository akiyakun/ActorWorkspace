using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using afl;
using afl.UI;
using afl.UI.v1;

namespace ActorWorkspace.InAppDebug
{
    public class UISkinControl : UIEntityGroup
    {
        [SerializeField] UIListView listView;

        public event System.Action<int> OnSkinChanged;

        protected override async UniTask<int> InnerInitializeAsync(CancellationToken cancellationToken)
        {
            listView.OnClick.AddListener(OnClickFromListView);
            return await UniTask.FromResult<int>(GeneralReturnCode.Succeeded);
        }

        protected override void InnerTerminate()
        {
            listView.OnClick.RemoveListener(OnClickFromListView);
        }

        public void ResetUI(IAWActor actor)
        {
            // SkeletonData skeletonData =  skeletonAnimation.Skeleton.Data;
            // ExposedList<Skin> skins = skeletonData.Skins;

            listView.Clear();

            var skinList = actor.SkinList;
            for (int i = 0; i < skinList.Count; i++)
            {
                var entity = listView.AddEntity();
                entity.Id = i;
                entity.name = skinList[i].Name;
                entity.GetComponentInChildren<TMP_Text>().text = skinList[i].Name;
                entity.SetStay();
            }
        }

        void OnClickFromListView(GameObject sender)
        {
            var entity = sender.GetComponent<UIEntity>();
            OnSkinChanged?.Invoke(entity.Id);
        }

    }
}
