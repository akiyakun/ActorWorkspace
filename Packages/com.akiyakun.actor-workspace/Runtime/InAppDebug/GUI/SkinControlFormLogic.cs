using UnityEngine;
using UnityEngine.UI;
using TMPro;
using afl;
using afl.UI;
using afl.UI.v1;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.InAppDebug
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
            }
        }

        void OnClickFromListView(GameObject sender)
        {
            var entity = sender.GetComponent<UIEntity>();
            OnSkinChanged?.Invoke(entity.Id);
        }

    }
}
