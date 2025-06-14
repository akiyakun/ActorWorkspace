using UnityEngine;
using UnityEngine.UI;
using TMPro;
using afl;
using afl.UI;
using afl.UI.v1;
using Spine;
using Spine.Unity;

namespace Project.InAppDebug
{
    public class SkinControlFormLogic : MonoBehaviour
    {
        [SerializeField] UIListView listView;

        public event System.Action<int> OnSkinChanged;

        void Awake()
        {
            listView.OnClick.AddListener(OnClickFromListView);
        }

        void OnDestroy()
        {
            listView.OnClick.RemoveListener(OnClickFromListView);
        }

        // fixme: 引数 skeletonAnimation
        public void ResetUI(SkeletonAnimation skeletonAnimation)
        {
            SkeletonData skeletonData =  skeletonAnimation.Skeleton.Data;
            ExposedList<Skin> skins = skeletonData.Skins;

            listView.Clear();

            for (int i = 0; i < skins.Count; i++)
            {
                var entity = listView.AddEntity();
                entity.Id = i;
                entity.name = skins.Items[i].Name;
                entity.GetComponentInChildren<TMP_Text>().text = skins.Items[i].Name;
            }
        }

        void OnClickFromListView(GameObject sender)
        {
            var entity = sender.GetComponent<UIEntity>();
            OnSkinChanged?.Invoke(entity.Id);
        }

    }
}
