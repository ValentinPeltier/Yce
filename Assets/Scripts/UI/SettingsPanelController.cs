using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class KeyDisplay {
    public KeyCode key;
    public string text;
    public Sprite image;
}

public class SettingsPanelController : PanelController {

	[SerializeField] private Transform elementsContent;
    [SerializeField] private Button applyButton;
    [SerializeField] private GameObject backConfirmation;
    [SerializeField] private GameObject twiceError;
    public KeyDisplay[] keyDisplay;

    [HideInInspector] public bool escapeKeyDisabled;

    // -----------------

    private Color elementNoErrorColor;
    private bool isOpen = false;

    // -----------------

    /// <summary>
    /// Load panel elements
    /// </summary>
    protected override void LoadPanel() {
        // Get main menu
        mainMenu = GameObject.Find("UI").GetComponent<MainMenu>();

        // Update elements values
        UpdateElementsValues();

        isLoaded = true;
    }

    /// <summary>
    /// Reset panel
    /// </summary>
    protected override void ResetPanel() {
        if (!isLoaded) {
            LoadPanel();
        }
    }

    /// <summary>
    /// On panel opening
    /// </summary>
    public override void OnOpen() {
        ResetPanel();

        UpdateElementsValues();

        // Set scrollbar value
        transform.Find("Content/ScrollSection/Scrollbar").GetComponent<Scrollbar>().value = 1.0f;

        mainMenu.escapeKeyDisabled = true;

        isOpen = true;
    }

    // ------------------

    /// <summary>
    /// Check for missing player preferences and fill them with default value
    /// And check for keys assigned multiple times
    /// </summary>
    public void InitializePreferences() {
        // For each settings element
        for(int i = 0, c = elementsContent.childCount; i < c; i++) {
            Transform child = elementsContent.GetChild(i);

            // If preference already exists
            if(PlayerPrefs.HasKey(child.name)) {
                // Skip it
                continue;
            }

            if(child.GetComponent<KeyElementController>()) {
                PlayerPrefs.SetInt(child.name, (int)child.GetComponent<KeyElementController>().defaultValue);
            }

            if (child.GetComponent<DropdownElementController>()) {
                PlayerPrefs.SetInt(child.name, child.GetComponent<DropdownElementController>().defaultValue);
            }

            if (child.GetComponent<SliderElementController>()) {
                PlayerPrefs.SetFloat(child.name, child.GetComponent<SliderElementController>().defaultValue);
            }
        }

        CheckForDoubleKey();
    }

    /// <summary>
    /// Update settings elements values
    /// </summary>
    public void UpdateElementsValues() {
        // For each element
        for (int i = 0, c = elementsContent.childCount; i < c; i++) {
            Transform element = elementsContent.GetChild(i);

            // If element is a dropdown element
            if (element.GetComponent<DropdownElementController>()) {
                element.GetComponent<DropdownElementController>().SetValue(PlayerPrefs.GetInt(element.name));
            }

            // If element is a slider element
            if (element.GetComponent<SliderElementController>()) {
                element.GetComponent<SliderElementController>().SetValue(PlayerPrefs.GetFloat(element.name));
            }

            // If element is a key element
            if (element.GetComponent<KeyElementController>()) {
                element.GetComponent<KeyElementController>().SetValue((KeyCode)PlayerPrefs.GetInt(element.name));
            }
        }
    }

    /// <summary>
    /// User changed the value of a settings element
    /// </summary>
    public void OnElementValueChanged() {
        // Set apply button to text
        applyButton.transform.Find("Text").gameObject.SetActive(true);
        applyButton.transform.Find("Checkmark").gameObject.SetActive(false);
    }

    /// <summary>
    /// Check for other key elements with the same value
    /// </summary>
    public void KeyElementValueChanged() {
        /* ------------------ */
        /* --- Get values --- */
        /* ------------------ */

        List<Pair<GameObject, KeyCode>> elements = new List<Pair<GameObject, KeyCode>>();

        // For each element
		for(int i = 0, ec = elementsContent.childCount; i < ec; i++) {
			GameObject element = elementsContent.GetChild(i).gameObject;
			
			KeyElementController keyComponent = element.GetComponent<KeyElementController>();
			
			// If element is a key element
			if(keyComponent != null) {
                // Add element to elements array
                elements.Add(new Pair<GameObject, KeyCode> {
                    e1 = element,
                    e2 = keyComponent.GetValue()
                });
			}
		}
        
        /* ---------------------- */
        /* --- Display errors --- */
        /* ---------------------- */
        int c = elements.Count;
        bool error = false;

        // Reset errors
        for (int i = 0; i < c; i++) {
            SetKeyElementError(elements[i].e1, false);
        }

        // For each element
        for (int i = 0; i < c; i++) {
            Pair<GameObject, KeyCode> element = elements[i];

            // If we can found an other element with the same key value
            for (int j = 0; j < c; j++) {
                // If two elements have the same key value
                if (i != j && elements[j].e2 == element.e2) {
                    // Display error
                    SetKeyElementError(elements[i].e1, true);
                    error = true;
                }
            }
		}

        if(error) {
            applyButton.interactable = false;
            twiceError.SetActive(true);
        }
        else {
            applyButton.interactable = true;
            twiceError.SetActive(false);
        }
    }

    /// <summary>
    /// User clicks on back button
    /// </summary>
    public void OnBackClick() {
        // Check if something has not been applied
        bool notApplied = false;

        for(int i = 0, c = elementsContent.childCount; i < c; i++) {
            Transform element = elementsContent.GetChild(i);

            // If element is a dropdown element
            if(element.GetComponent<DropdownElementController>()) {
                // Check if its value has been changed
                if(element.GetComponent<DropdownElementController>().GetValue() != PlayerPrefs.GetInt(element.name)) {
                    notApplied = true;
                    break;
                }
            }

            // If element is a slider element
            if (element.GetComponent<SliderElementController>()) {
                // Check if its value has been changed
                if (element.GetComponent<SliderElementController>().GetValue() != PlayerPrefs.GetFloat(element.name)) {
                    notApplied = true;
                    break;
                }
            }

            // If element is a key element
            if (element.GetComponent<KeyElementController>()) {
                // Check if its value has been changed
                if ((int)element.GetComponent<KeyElementController>().GetValue() != PlayerPrefs.GetInt(element.name)) {
                    notApplied = true;
                    break;
                }
            }
        }

        if (notApplied) {
            backConfirmation.SetActive(true);
        }
        else {
            // Close settings panel
            mainMenu.escapeKeyDisabled = false;
            mainMenu.CloseCurrentPanel();
        }
    }

    /// <summary>
    /// Save and apply every settings element
    /// </summary>
    public void OnApplyClick() {
        // Save
        SaveChanges();

        // Apply
        ApplyChanges();

        // Set apply button to checkmark
        applyButton.transform.Find("Text").gameObject.SetActive(false);
        applyButton.transform.Find("Checkmark").gameObject.SetActive(true);
    }

    /// <summary>
    /// User clicks on yes button in back confirmation
    /// </summary>
    public void OnBackConfirmationYesClick() {
        backConfirmation.SetActive(false);

        // Close settings panel
        mainMenu.escapeKeyDisabled = false;
        mainMenu.CloseCurrentPanel();
    }

    /// <summary>
    /// User clicks on no button in back confirmation
    /// </summary>
    public void OnBackConfirmationNoClick() {
        backConfirmation.SetActive(false);
    }

    // ------------------

    /// <summary>
    /// Check for twice same value in player preferences (or more) and set their value to default value
    /// </summary>
    private void CheckForDoubleKey() {
        // For each settings element
        for (int i = 0, ic = elementsContent.childCount; i < ic; i++) {
            Transform child0 = elementsContent.GetChild(i);
            KeyElementController child0KeyComponent = child0.GetComponent<KeyElementController>();

            // If no key element controller attached to it
            if (child0KeyComponent == null) {
                // Skip it
                continue;
            }

            // Search for other key elements with the same value
            for (int j = 0, jc = elementsContent.childCount; j < jc; j++) {
                Transform child1 = elementsContent.GetChild(j);
                KeyElementController child1KeyComponent = child1.GetComponent<KeyElementController>();

                // If no key element controller attached to it
                if (child1KeyComponent == null) {
                    // Skip it
                    continue;
                }

                // If same value exists twice
                if (child0KeyComponent.GetValue() == child1KeyComponent.GetValue()) {
                    // Set their value to default value
                    PlayerPrefs.SetInt(child0.name, (int)child0KeyComponent.defaultValue);
                    PlayerPrefs.SetInt(child1.name, (int)child1KeyComponent.defaultValue);
                }
            }
        }
    }

    private void ApplyChanges() {
        // Window mode
        bool fullScreen = PlayerPrefs.GetInt("Window") == 0 ? false : true;
		Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, fullScreen);

        // Audio volume
        AudioListener.volume = PlayerPrefs.GetFloat("Volume") / 100.0f;
	}

    private void SaveChanges() {
        // For each settings element
        for (int i = 0, c = elementsContent.childCount; i < c; i++) {
            GameObject element = elementsContent.GetChild(i).gameObject;

            // If element is a KeyElement
            if (element.GetComponent<KeyElementController>()) {
                int value = (int)element.GetComponent<KeyElementController>().GetValue();

                // Save value into PlayerPrefs
                PlayerPrefs.SetInt(element.name, value);
            }
            // If element is a SliderElement
            if (element.GetComponent<SliderElementController>()) {
                float value = element.GetComponent<SliderElementController>().GetValue();

                // Save value into PlayerPrefs
                PlayerPrefs.SetFloat(element.name, value);
            }
            // If element is a DropdownElement
            if (element.GetComponent<DropdownElementController>()) {
                int value = element.GetComponent<DropdownElementController>().GetValue();

                // Save value into PlayerPrefs
                PlayerPrefs.SetInt(element.name, value);
            }
        }
    }

    private void SetKeyElementError(GameObject keyElement, bool error) {
        // Get color
        Color color = error ? new Color(1.0f, 0.0f, 0.0f) : elementNoErrorColor;

        keyElement.transform.Find("Text").GetComponent<Text>().color = color;
    }

    // ------------------

    private void Awake() {
        // Hide
        backConfirmation.SetActive(false);
        twiceError.SetActive(false);

        // Set elementNoErrorColor
        for (int i = 0, c = elementsContent.childCount; i < c; i++) {
            Transform element = elementsContent.GetChild(i);
            if (element.GetComponent<DropdownElementController>() || element.GetComponent<SliderElementController>() || element.GetComponent<KeyElementController>()) {
                elementNoErrorColor = element.Find("Text").GetComponent<Text>().color;
                break;
            }
        }
    }

    private void Update() {
        // If user clicks on escape key
        if(isOpen && !escapeKeyDisabled && Input.GetKeyDown(KeyCode.Escape)) {
            OnBackClick();
        }
    }
}