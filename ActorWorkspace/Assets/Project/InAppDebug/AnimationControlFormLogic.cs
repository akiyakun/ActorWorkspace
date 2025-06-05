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

        // [SerializeField]
        public event System.Action<float> OnSpeedValueChanged;

        void Awake()
        {
            speedInputField = speedControl.Find("InputField").GetComponent<TMP_InputField>();
            speedControl.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(OnSliderValueChangedFromSpeedControl);
            speedControl.Find("Reset").GetComponent<Button>().onClick.AddListener(OnResetFromSpeedControl);
        }

        void OnDestroy()
        {
            speedControl.Find("Slider").GetComponent<Slider>().onValueChanged.RemoveListener(OnSliderValueChangedFromSpeedControl);
            speedControl.Find("Reset").GetComponent<Button>().onClick.RemoveListener(OnResetFromSpeedControl);
        }

        #region SpeedControl
        void OnSliderValueChangedFromSpeedControl(float value)
        {
            Debug.Log($"Speed changed to: {value}");
            speedInputField.text = Mathf.RoundToInt(value * 100.0f).ToString();
            OnSpeedValueChanged?.Invoke(value);
        }

        void OnResetFromSpeedControl()
        {
            OnSliderValueChangedFromSpeedControl(1.0f);
        }
        #endregion
    }
}
