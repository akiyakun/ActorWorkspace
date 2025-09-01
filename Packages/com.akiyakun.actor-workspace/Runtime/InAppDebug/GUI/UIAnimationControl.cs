using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using afl;
using afl.UI;

namespace ActorWorkspace.InAppDebug
{
    public class UIAnimationControl : UIEntityRootGroup
    {
        [SerializeField] GameObject speedControl;
        TMP_InputField speedInputField;

        [SerializeField] GameObject mixControl;
        TMP_InputField mixInputField;
        Toggle loopToggle;

        [SerializeField] GameObject trackControl;

        public event System.Action<float> OnSpeedValueChanged;
        public event System.Action<float> OnMixValueChanged;
        public event System.Action<bool> OnLoopValueChanged;
        public event System.Action<int> OnActiveTrackChanged;

        // public event System.Action<int> OnAnimationChanged;


        public bool IsLoop => loopToggle.isOn;

        protected override async UniTask<int> InnerInitializeAsync(CancellationToken cancellationToken)
        {
            speedInputField = speedControl.GetComponent<TMP_InputField>("InputField");

            mixInputField = mixControl.GetComponent<TMP_InputField>("InputField");
            loopToggle = mixControl.GetComponent<Toggle>("Loop");

            // イベント登録
            {
                speedControl.GetComponent<Slider>("Slider").onValueChanged.AddListener(OnSpeedSliderValueChanged);
                speedControl.GetComponent<Button>("Reset").onClick.AddListener(OnSpeedReset);

                mixControl.GetComponent<Slider>("Slider").onValueChanged.AddListener(OnMixSliderValueChanged);
                mixControl.GetComponent<Toggle>("Loop").onValueChanged.AddListener(OnLoopToggleChanged);

                trackControl.GetComponent<Toggle>("Track0").onValueChanged.AddListener(OnTrack0ToggleChanged);
                trackControl.GetComponent<Toggle>("Track1").onValueChanged.AddListener(OnTrack1ToggleChanged);
                trackControl.GetComponent<Toggle>("Track2").onValueChanged.AddListener(OnTrack2ToggleChanged);
                trackControl.GetComponent<Toggle>("Track3").onValueChanged.AddListener(OnTrack3ToggleChanged);
                // trackControl.GetComponent<Toggle>("Track4").onValueChanged.AddListener(OnTrack4ToggleChanged);

                // OnAnimationChanged += InnerOnAnimationChanged;
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
                speedControl.GetComponent<Slider>("Slider").onValueChanged.RemoveListener(OnSpeedSliderValueChanged);
                speedControl.GetComponent<Button>("Reset").onClick.RemoveListener(OnSpeedReset);

                mixControl.GetComponent<Toggle>("Loop").onValueChanged.RemoveListener(OnLoopToggleChanged);

                trackControl.GetComponent<Toggle>("Track0").onValueChanged.RemoveListener(OnTrack0ToggleChanged);
                trackControl.GetComponent<Toggle>("Track1").onValueChanged.RemoveListener(OnTrack1ToggleChanged);
                trackControl.GetComponent<Toggle>("Track2").onValueChanged.RemoveListener(OnTrack2ToggleChanged);
                trackControl.GetComponent<Toggle>("Track3").onValueChanged.RemoveListener(OnTrack3ToggleChanged);
                trackControl.GetComponent<Toggle>("Track4").onValueChanged.RemoveListener(OnTrack4ToggleChanged);

                // OnAnimationChanged -= InnerOnAnimationChanged;
            }
        }

        void ResetUI(IAWActor actor)
        {
            OnSpeedReset();
            OnMixSliderValueChanged(0.25f);
            OnLoopToggleChanged(IsLoop);
            ResetTrackControl();
        }

        // public void Refresh(bool loop)

        public void ResetTrackControl()
        {
            for (int i = 0; i < 5; i++)
            {
                var defaultText = trackControl.GetComponent<TMP_Text>($"Track{i}/Background/Checkmark/DefaultText");
                var playingText = trackControl.GetComponent<TMP_Text>($"Track{i}/Background/Checkmark/PlayingText");

                defaultText.gameObject.SetActive(true);
                playingText.gameObject.SetActive(false);
            }

            trackControl.GetComponent<Toggle>($"Track{0}").isOn = true;
        }

        // 負の値は停止あつかい
        public void TrackAnimationChanged(int trackIndex, bool isPlaying)
        {
            // for (int i = 0; i < 5; i++)
            {
                int i = trackIndex;
                var defaultText = trackControl.GetComponent<TMP_Text>($"Track{i}/Background/Checkmark/DefaultText");
                var playingText = trackControl.GetComponent<TMP_Text>($"Track{i}/Background/Checkmark/PlayingText");

                defaultText.gameObject.SetActive(!isPlaying);
                playingText.text = defaultText.text;
                playingText.gameObject.SetActive(isPlaying);

                // if (i != trackIndex)
                // {
                //     defaultText.gameObject.SetActive(true);
                //     playingText.gameObject.SetActive(false);
                // }
                // else
                // {
                //     defaultText.gameObject.SetActive(false);
                //     playingText.text = defaultText.text;
                //     playingText.gameObject.SetActive(true);
                // }
            }
        }

        public void ChangeTrack(float speed, float mix)
        {
            speedControl.GetComponentInChildren<Slider>().value = speed;
            mixControl.GetComponentInChildren<Slider>().value = mix;
        }


        void OnSpeedSliderValueChanged(float value)
        {
            speedInputField.text = Mathf.RoundToInt(value * 100.0f).ToString();
            OnSpeedValueChanged?.Invoke(value);
        }

        void OnSpeedReset()
        {
            OnSpeedSliderValueChanged(1.0f);
        }

        void OnMixSliderValueChanged(float value)
        {
            // Debug.Log($"OnMixSliderValueChanged");
            mixInputField.text = value.ToString("F2");
            OnMixValueChanged?.Invoke(value);
        }

        void OnLoopToggleChanged(bool value)
        {
            loopToggle.isOn = value;
            OnLoopValueChanged?.Invoke(value);
        }

        void OnTrack0ToggleChanged(bool value)
        {
            OnActiveTrackChanged?.Invoke(0);
        }

        void OnTrack1ToggleChanged(bool value)
        {
            OnActiveTrackChanged?.Invoke(1);
        }

        void OnTrack2ToggleChanged(bool value)
        {
            OnActiveTrackChanged?.Invoke(2);
        }

        void OnTrack3ToggleChanged(bool value)
        {
            OnActiveTrackChanged?.Invoke(3);
        }

        void OnTrack4ToggleChanged(bool value)
        {
            OnActiveTrackChanged?.Invoke(4);
        }

    }
}
