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
    public class AnimationListFormLogic : MonoBehaviour
    {
        [SerializeField] UIListView listView;

        // [SerializeField] GameObject speedControl;
        // TMP_InputField speedInputField;

        // [SerializeField] GameObject mixControl;
        // TMP_InputField mixInputField;
        // Toggle loopToggle;

        // [SerializeField] GameObject trackControl;

        public event System.Action<UIEntity> OnClickEntity;
        // public event System.Action<float> OnMixValueChanged;
        // public event System.Action<bool> OnLoopValueChanged;
        // public event System.Action<int> OnActiveTrackChanged;

        // // public event System.Action<int> OnAnimationChanged;


        // public bool IsLoop => loopToggle.isOn;

        void Awake()
        {
            //     speedInputField = speedControl.Find("InputField").GetComponent<TMP_InputField>();

            //     mixInputField = mixControl.Find("InputField").GetComponent<TMP_InputField>();
            //     loopToggle = mixControl.Find("Loop").GetComponent<Toggle>();

            // イベント登録
            {
                listView.OnClick.AddListener(OnClickFromListView);

                //         speedControl.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(OnSpeedSliderValueChanged);
                //         speedControl.Find("Reset").GetComponent<Button>().onClick.AddListener(OnSpeedReset);

                //         mixControl.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(OnMixSliderValueChanged);
                //         mixControl.Find("Loop").GetComponent<Toggle>().onValueChanged.AddListener(OnLoopToggleChanged);

                //         trackControl.Find("Track0").GetComponent<Toggle>().onValueChanged.AddListener(OnTrack0ToggleChanged);
                //         trackControl.Find("Track1").GetComponent<Toggle>().onValueChanged.AddListener(OnTrack1ToggleChanged);
                //         trackControl.Find("Track2").GetComponent<Toggle>().onValueChanged.AddListener(OnTrack2ToggleChanged);
                //         trackControl.Find("Track3").GetComponent<Toggle>().onValueChanged.AddListener(OnTrack3ToggleChanged);
                //         trackControl.Find("Track4").GetComponent<Toggle>().onValueChanged.AddListener(OnTrack4ToggleChanged);

                //         // OnAnimationChanged += InnerOnAnimationChanged;
            }
        }

        void OnDestroy()
        {
            // イベント解除
            {
                listView.OnClick.RemoveListener(OnClickFromListView);

                //         speedControl.Find("Slider").GetComponent<Slider>().onValueChanged.RemoveListener(OnSpeedSliderValueChanged);
                //         speedControl.Find("Reset").GetComponent<Button>().onClick.RemoveListener(OnSpeedReset);

                //         mixControl.Find("Loop").GetComponent<Toggle>().onValueChanged.RemoveListener(OnLoopToggleChanged);

                //         trackControl.Find("Track0").GetComponent<Toggle>().onValueChanged.RemoveListener(OnTrack0ToggleChanged);
                //         trackControl.Find("Track1").GetComponent<Toggle>().onValueChanged.RemoveListener(OnTrack1ToggleChanged);
                //         trackControl.Find("Track2").GetComponent<Toggle>().onValueChanged.RemoveListener(OnTrack2ToggleChanged);
                //         trackControl.Find("Track3").GetComponent<Toggle>().onValueChanged.RemoveListener(OnTrack3ToggleChanged);
                //         trackControl.Find("Track4").GetComponent<Toggle>().onValueChanged.RemoveListener(OnTrack4ToggleChanged);

                //         // OnAnimationChanged -= InnerOnAnimationChanged;
                //     }
            }
        }

        public void ResetUI(SkeletonAnimation skeletonAnimation)
        {
            ResetListView(skeletonAnimation);
        }

        public void ResetListView(SkeletonAnimation skeletonAnimation)
        {
            listView.Clear();

            SkeletonData skeletonData = skeletonAnimation.Skeleton.Data;
            for (int i = 0; i < skeletonData.Animations.Count; i++)
            {
                var animation = skeletonData.Animations.Items[i];
                var entity = listView.AddEntity();
                entity.Id = i;
                entity.UserData = animation;
                entity.transform.Find("DefaultButton/Text").GetComponent<TMP_Text>().text = animation.Name;
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
            var entityList = listView.ContentRoot.GetEntityList();
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
            OnClickEntity?.Invoke(entity);
        }

    }
}
