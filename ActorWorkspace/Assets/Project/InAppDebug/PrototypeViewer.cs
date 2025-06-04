using UnityEngine;
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
        public GameObject openAssetDialog;

        public UIListView uiListView;

        public SkeletonAnimation skeletonAnimation;

        public IActorAssetDatabase ActorAssetDatabase { get; set; }

        public WorkingActorContext CurrentWorkingActorContext { get; set; }

        void Awake()
        {
            // openAssetDialog.SetActive(false);
        }

        async void Start()
        {
            {
                var group = openAssetDialog.Find("UIListView").GetComponent<UIEntityGroup>();
                await group.Initialize(UIContextProvider.Default, this.destroyCancellationToken);
            }

            var g = uiListView.GetComponent<UIEntityGroup>();
            await g.Initialize(UIContextProvider.Default, this.destroyCancellationToken);
            // Debug.Log("UIEntityGroup initialized");

            // uiListView.AddItem();
            // uiListView.AddItem();

            SkeletonData skeletonData = skeletonAnimation.Skeleton.Data;
            foreach (Spine.Animation animation in skeletonData.Animations)
            {
                // Debug.Log("Animation name: " + animation.Name);
                var obj = uiListView.AddEntity();
                obj.GetComponentInChildren<TMP_Text>().text = animation.Name;
            }

        }

        public void LoadActorAsset(string path)
        {
            SpineHelper.CreateSkeletonGameObjectFromAsset(path);
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

            if (CurrentWorkingActorContext != null)
            {
                CurrentWorkingActorContext.Release();
                CurrentWorkingActorContext = null;
            }

            var skeletonGameObject = SpineHelper.CreateSkeletonGameObjectFromAsset(assetInfo.Path);
            CurrentWorkingActorContext = new();
            CurrentWorkingActorContext.GameObject = skeletonGameObject;

            openAssetDialog.SetActive(false);
        }

    }
}