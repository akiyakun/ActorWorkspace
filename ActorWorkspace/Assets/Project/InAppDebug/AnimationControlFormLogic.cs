using UnityEngine;
using UnityEngine.UI;
using TMPro;
using afl;

namespace Project.InAppDebug
{
    public class AnimationControlFormLogic : MonoBehaviour
    {
        [SerializeField] GameObject speedControl;
        TMP_InputField speedInputField;

        [SerializeField] GameObject mixControl;
        Toggle loopToggle;

        public event System.Action<float> OnSpeedValueChanged;
        public event System.Action<bool> OnLoopValueChanged;

        public bool IsLoop => loopToggle.isOn;

        void Awake()
        {
            speedInputField = speedControl.Find("InputField").GetComponent<TMP_InputField>();

            loopToggle = mixControl.Find("Loop").GetComponent<Toggle>();


            speedControl.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(OnSpeedSliderValueChanged);
            speedControl.Find("Reset").GetComponent<Button>().onClick.AddListener(OnSpeedReset);

            mixControl.Find("Loop").GetComponent<Toggle>().onValueChanged.AddListener(OnLoopToggleChanged);
        }

        void OnDestroy()
        {
            speedControl.Find("Slider").GetComponent<Slider>().onValueChanged.RemoveListener(OnSpeedSliderValueChanged);
            speedControl.Find("Reset").GetComponent<Button>().onClick.RemoveListener(OnSpeedReset);

            mixControl.Find("Loop").GetComponent<Toggle>().onValueChanged.RemoveListener(OnLoopToggleChanged);
        }

        public void Setup(bool loop)
        {
            OnSpeedReset();
            OnLoopToggleChanged(loop);
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

        void OnLoopToggleChanged(bool value)
        {
            loopToggle.isOn = value;
            OnLoopValueChanged?.Invoke(value);
        }
    }
}
