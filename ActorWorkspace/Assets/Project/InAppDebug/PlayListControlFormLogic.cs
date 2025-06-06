using UnityEngine;
using UnityEngine.UI;
using TMPro;
using afl;

namespace Project.InAppDebug
{
    public class PlayListControlFormLogic : MonoBehaviour
    {
        // [SerializeField] GameObject speedControl;
        // TMP_InputField speedInputField;

        // [SerializeField] GameObject mixControl;
        // TMP_InputField mixInputField;
        // Toggle loopToggle;

        // [SerializeField] GameObject trackControl;

        // public event System.Action<float> OnSpeedValueChanged;
        // public event System.Action<float> OnMixValueChanged;
        // public event System.Action<bool> OnLoopValueChanged;
        // public event System.Action<int> OnActiveTrackChanged;

        // // public event System.Action<int> OnAnimationChanged;


        // public bool IsLoop => loopToggle.isOn;

        // void Awake()
        // {
        //     speedInputField = speedControl.Find("InputField").GetComponent<TMP_InputField>();

        //     mixInputField = mixControl.Find("InputField").GetComponent<TMP_InputField>();
        //     loopToggle = mixControl.Find("Loop").GetComponent<Toggle>();

        //     // イベント登録
        //     {
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
        //     }
        // }

        // void OnDestroy()
        // {
        //     // イベント解除
        //     {
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
        // }

        // public void ResetUI(bool loop)
        // {
        //     OnSpeedReset();
        //     OnMixSliderValueChanged(0.25f);
        //     OnLoopToggleChanged(loop);
        //     ResetTrackControl();
        // }

        // // public void Refresh(bool loop)

        // public void ResetTrackControl()
        // {
        //     for (int i = 0; i < 5; i++)
        //     {
        //         var defaultText = trackControl.Find($"Track{i}/Background/Checkmark/DefaultText").GetComponent<TMP_Text>();
        //         var playingText = trackControl.Find($"Track{i}/Background/Checkmark/PlayingText").GetComponent<TMP_Text>();

        //         defaultText.gameObject.SetActive(true);
        //         playingText.gameObject.SetActive(false);
        //     }

        //     trackControl.Find($"Track{0}").GetComponent<Toggle>().isOn = true;
        // }

        // // 負の値は停止あつかい
        // public void TrackAnimationChanged(int trackIndex, bool isPlaying)
        // {
        //     // for (int i = 0; i < 5; i++)
        //     {
        //         int i = trackIndex;
        //         var defaultText = trackControl.Find($"Track{i}/Background/Checkmark/DefaultText").GetComponent<TMP_Text>();
        //         var playingText = trackControl.Find($"Track{i}/Background/Checkmark/PlayingText").GetComponent<TMP_Text>();

        //         defaultText.gameObject.SetActive(!isPlaying);
        //         playingText.text = defaultText.text;
        //         playingText.gameObject.SetActive(isPlaying);

        //         // if (i != trackIndex)
        //         // {
        //         //     defaultText.gameObject.SetActive(true);
        //         //     playingText.gameObject.SetActive(false);
        //         // }
        //         // else
        //         // {
        //         //     defaultText.gameObject.SetActive(false);
        //         //     playingText.text = defaultText.text;
        //         //     playingText.gameObject.SetActive(true);
        //         // }
        //     }
        // }


        // void OnSpeedSliderValueChanged(float value)
        // {
        //     speedInputField.text = Mathf.RoundToInt(value * 100.0f).ToString();
        //     OnSpeedValueChanged?.Invoke(value);
        // }

        // void OnSpeedReset()
        // {
        //     OnSpeedSliderValueChanged(1.0f);
        // }

        // void OnMixSliderValueChanged(float value)
        // {
        //     // Debug.Log($"OnMixSliderValueChanged");
        //     mixInputField.text = value.ToString("F2");
        //     OnMixValueChanged?.Invoke(value);
        // }

        // void OnLoopToggleChanged(bool value)
        // {
        //     loopToggle.isOn = value;
        //     OnLoopValueChanged?.Invoke(value);
        // }

        // void OnTrack0ToggleChanged(bool value)
        // {
        //     OnActiveTrackChanged?.Invoke(0);
        // }

        // void OnTrack1ToggleChanged(bool value)
        // {
        //     OnActiveTrackChanged?.Invoke(1);
        // }

        // void OnTrack2ToggleChanged(bool value)
        // {
        //     OnActiveTrackChanged?.Invoke(2);
        // }

        // void OnTrack3ToggleChanged(bool value)
        // {
        //     OnActiveTrackChanged?.Invoke(3);
        // }

        // void OnTrack4ToggleChanged(bool value)
        // {
        //     OnActiveTrackChanged?.Invoke(4);
        // }

    }
}
