using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyElementController : MonoBehaviour {

    public KeyCode defaultValue;

    // ---------------

	private SettingsPanelController settingsPanelController;
	private ScreenshotController screenshotController;
	
    private KeyDisplay[] keyDisplay;

    private GameObject overlay;

    private Button button;
    private Text text;
    private Image image;

    private bool buttonActive = false;

    private KeyCode value;

    // -------------------

    /// <summary>
    /// Return key value
    /// </summary>
    public KeyCode GetValue() {
        return value;
    }

    /// <summary>
    /// Set key value
    /// </summary>
    /// <param name="value">The value of the key to set</param>
    public void SetValue(KeyCode value) {
        this.value = value;

        // Update button display values
        SetButtonDisplay(GetKeyDisplay(value));
		
		// Call settings panel controller KeyElementValueChanged and OnElementValueChanged functions
		settingsPanelController.KeyElementValueChanged();
        settingsPanelController.OnElementValueChanged();
    }

    // -------------------

    /// <summary>
    /// Set button text value
    /// </summary>
    /// <param name="buttonText">The text to display on the button</param>
    private void SetButtonText(string buttonText) {
        text.text = buttonText;
        text.gameObject.SetActive(true);

        image.gameObject.SetActive(false);
    }

    /// <summary>
    /// Set button image value
    /// </summary>
    /// <param name="buttonImage">The image to display on the button</param>
    private void SetButtonImage(Sprite buttonImage) {
        image.sprite = buttonImage;
        image.gameObject.SetActive(true);

        text.gameObject.SetActive(false);
    }

    /// <summary>
    /// Set button display values
    /// </summary>
    /// <param name="buttonKeyDisplay">The KeyDisplay to display on the button</param>
    private void SetButtonDisplay(KeyDisplay buttonKeyDisplay) {
        // Check if no error
        if(buttonKeyDisplay == null) {
            return;
        }

        if (buttonKeyDisplay.text != "") {
            // Set button text
            SetButtonText(buttonKeyDisplay.text);
        }
        else if (buttonKeyDisplay.image != null) {
            // Set button image
            SetButtonImage(buttonKeyDisplay.image);
        }
        else {
            // No special display for this key
            SetButtonText(buttonKeyDisplay.key.ToString().ToUpper());
        }
    }

    /// <summary>
    /// Set button state
    /// </summary>
    /// <param name="state">The state of the button</param>
    private void SetButtonState(bool state) {
        buttonActive = state;

        // Set overlay state
        overlay.SetActive(state);

        // Activate/deactivate mainMenu escape key
        settingsPanelController.escapeKeyDisabled = state;
		
		// Activate/deactivate screenshot key
		screenshotController.screenshotKeyDisabled = state;
    }

    /// <summary>
    /// User clicks on button
    /// </summary>
    private void OnButtonClick() {
        // Activate button
        SetButtonState(true);
    }

    /// <summary>
    /// Return keyDisplay of keyCode, null if keyCode is incorrect
    /// </summary>
    /// <param name="keyCode">The keyCode to get its keyDisplay</param>
    private KeyDisplay GetKeyDisplay(KeyCode keyCode) {
        for(int i = 0, c = keyDisplay.Length; i < c; i++) {
            if(keyDisplay[i].key == keyCode) {
                return keyDisplay[i];
            }
        }

        return null;
    }

    // -------------------

    private void Awake() {
        settingsPanelController = GameObject.Find("UI").transform.Find("MenuCanvas/Panels/Settings").GetComponent<SettingsPanelController>();
		screenshotController = GameObject.Find("Camera").GetComponent<ScreenshotController>();
		
        keyDisplay = settingsPanelController.keyDisplay;

        overlay = settingsPanelController.transform.Find("Overlay").gameObject;

        button = transform.Find("Button").GetComponent<Button>();
        text = transform.Find("Button/Text").GetComponent<Text>();
        image = transform.Find("Button/Image").GetComponent<Image>();

        // Add onClick listener
        button.onClick.AddListener(OnButtonClick);

        // Disable overlay
        overlay.SetActive(false);

        // Set value to defaultValue
        SetValue(defaultValue);
    }

    private void Update() {
        if (buttonActive && Input.anyKeyDown) {
            // Find which key is pressed
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode))) {
                if(Input.GetKeyDown(keyCode)) {
                    // Key found
                    if(keyCode == KeyCode.Escape) {
                        // Set button text/image as before
                        SetButtonDisplay(GetKeyDisplay(value));

                        // Deactivate button
                        SetButtonState(false);
                    }

                    // Set button text/image
                    KeyDisplay buttonKeyDisplay = GetKeyDisplay(keyCode);

                    // If key is correct
                    if (buttonKeyDisplay != null) {
                        // Set button value and display
                        SetValue(keyCode);

                        // Deactivate button
                        SetButtonState(false);

                        break;
                    }
                }
            }
        }
    }
}
