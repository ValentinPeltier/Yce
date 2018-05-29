using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderElementController : MonoBehaviour {

    public float defaultValue;

    // -----------------

    [System.Serializable]
    private enum SliderType {
        Integer, Float
    }

    [SerializeField] private SliderType sliderType;
	[SerializeField] private float maxValue;

    private Slider slider;
    private InputField inputField;

    // ------------------

    /// <summary>
    /// Return slider value
    /// </summary>
    public float GetValue() {
        float value;

        if (float.TryParse(inputField.text, out value)) {
            return value;
        }

        return 0.0f;
    }

    /// <summary>
    /// Set slider element value
    /// </summary>
    /// <param name="value">The value to set to the slider element</param>
    public void SetValue(float value) {
		// If sliderType is int
		if(sliderType == SliderType.Integer) {
			// Parse it to an integer
			value = (int)value;
		}
		
		// Clamp value
		value = Mathf.Clamp(value, 0.0f, maxValue);

		// Set slider value
		slider.value = value / maxValue;
		
		// Set inputField value
		inputField.text = value.ToString();
    }

    // -------------------

	/// <summary>
	/// On slider value changed, set inputField value
	/// </summary>
    private void SliderValueChanged(float sliderValue) {		
		float value = sliderValue * maxValue;
		
		// If sliderType is integer, parse value as integer
		if(sliderType == SliderType.Integer) {
			value = (int)value;
		}
		
		// Set inputField value	
		inputField.text = value.ToString();
    }

	/// <summary>
	/// On inputField value changed, set slider value
	/// </summary>
    private void InputFieldValueChanged(string inputFieldValue) {		
		// Parse value to float
		float fValue;

		if(!float.TryParse(inputFieldValue, out fValue)) {
			// Not successfully parsed
			// Set input field text to the previous value
            inputField.text = (slider.value * maxValue).ToString();

            return;
        }
		
		// Clamp value
		if(fValue < 0.0f || fValue > maxValue) {
			fValue = Mathf.Clamp(fValue, 0.0f, maxValue);
			
			// If sliderType is integer, parse fValue as integer
			if(sliderType == SliderType.Integer) {
				fValue = (int)fValue;
			}
		
			inputField.text = fValue.ToString();
		}
		
		// Set slider value
		slider.value = fValue / maxValue;
    }

    // -------------------

    private void Awake() {
        slider = transform.Find("Slider").GetComponent<Slider>();
        inputField = transform.Find("InputField").GetComponent<InputField>();
		
		// If slider type is an int
		if(sliderType == SliderType.Integer) {
			// Parse maxValue to an int
			maxValue = (int)maxValue;
		}
		
		// Maximum value cannot be 0
		if(maxValue == 0) {
			Debug.LogWarning("maxValue cannot be 0 (in " + gameObject.name + " sliderElement)");
		}

        // Set inputField contentType
        if (sliderType == SliderType.Integer) {
            inputField.contentType = InputField.ContentType.IntegerNumber;
        }
        else if (sliderType == SliderType.Float) {
            inputField.contentType = InputField.ContentType.DecimalNumber;
        }

        // Set inputField character limit
        inputField.characterLimit = maxValue.ToString().Length + 1;

        // Set listeners
        slider.onValueChanged.AddListener(SliderValueChanged);
        inputField.onValueChanged.AddListener(InputFieldValueChanged);

        // Set value to default value
        SetValue(defaultValue);
        
        // Add value changed listener
        SettingsPanelController settingsPanelController = GameObject.Find("UI").transform.Find("MenuCanvas/Panels/Settings").GetComponent<SettingsPanelController>();
        slider.onValueChanged.AddListener((x) => settingsPanelController.OnElementValueChanged());
    }
}
