using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownElementController : MonoBehaviour {

    public int defaultValue;

    // ---------------

    [SerializeField] private List<string> options;

    private Dropdown dropdown;

    // -------------------

    /// <summary>
    /// Return dropdown value
    /// </summary>
    public int GetValue() {
        return dropdown.value;
    }

    /// <summary>
    /// Set dropdown value
    /// </summary>
    /// <param name="value">The value to set</param>
    public void SetValue(int value) {
        dropdown.value = value;
    }

    // -------------------

    private void Awake() {
        dropdown = transform.Find("Dropdown").GetComponent<Dropdown>();

        // Set dropdown options
        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        // Set label text to defaultValue
        transform.Find("Dropdown/Label").GetComponent<Text>().text = options[defaultValue];

        // Set value to default value
        SetValue(defaultValue);

        // Add value changed listener
        SettingsPanelController settingsPanelController = GameObject.Find("UI").transform.Find("MenuCanvas/Panels/Settings").GetComponent<SettingsPanelController>();
        dropdown.onValueChanged.AddListener((x) => settingsPanelController.OnElementValueChanged());
    }
}
