using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPanelController : PanelController {

	private SaveController saveController;

	private GameObject content;
	private GameObject quitConfirmation;

	private Transform saveButton;

    private bool quitConfirmationActive = false;

    // ------------------

    /// <summary>
    /// Load panel elements
    /// </summary>
    protected override void LoadPanel() {
        // Get UI
        GameObject UI = GameObject.Find("UI");

        mainMenu = UI.GetComponent<MainMenu>();
        saveController = UI.GetComponent<SaveController>();
        content = transform.Find("Content").gameObject;
        quitConfirmation = transform.Find("QuitConfirmation").gameObject;

        saveButton = content.transform.Find("SaveButton");

        content.SetActive(true);
        quitConfirmation.SetActive(false);
        saveButton.Find("Text").gameObject.SetActive(true);
        saveButton.Find("NoError").gameObject.SetActive(false);

        isLoaded = true;
    }

    /// <summary>
    /// Reset panel
    /// </summary>
    protected override void ResetPanel() {
        if (!isLoaded) {
            LoadPanel();
        }

        // Reset confirmation
        quitConfirmation.SetActive(false);
        content.SetActive(true);

        // Reset buttons
        saveButton.Find("Text").gameObject.SetActive(true);
        saveButton.Find("NoError").gameObject.SetActive(false);
    }

    /// <summary>
    /// On panel opening
    /// </summary>
    public override void OnOpen() {
        ResetPanel();
    }

    // ------------------

    /// <summary>
    /// User clicks on back button
    /// </summary>
	public void OnBackClick() {
		mainMenu.CloseCurrentPanel();
	}

    /// <summary>
    /// User clicks on save button
    /// </summary>
	public void OnSaveClick() {
        saveController.Save();

        // Display no error
		saveButton.Find("Text").gameObject.SetActive(false);
		saveButton.Find("NoError").gameObject.SetActive(true);
	}

    /// <summary>
    /// User clicks on settings button
    /// </summary>
	public void OnSettingsClick() {
		mainMenu.OpenPanel(mainMenu.GetPanel("Settings"));
	}

    /// <summary>
    /// User clicks on quit button
    /// </summary>
	public void OnQuitClick() {
        content.SetActive(false);
		quitConfirmation.SetActive(true);

        quitConfirmationActive = true;

        mainMenu.escapeKeyDisabled = true;
    }

    /// <summary>
    /// User clicks on confirm button in quit confirmation panel
    /// </summary>
	public void OnQuitConfirmationConfirmClick() {
		Application.Quit();
	}

    /// <summary>
    /// User clicks on cancel button in quit confirmation panel
    /// </summary>
    public void OnQuitConfirmationCancelClick() {
        content.SetActive(true);
        quitConfirmation.SetActive(false);

        quitConfirmationActive = false;
        
        StartCoroutine(WaitForNextFrameAndEnableEscapeKey());
    }

    private IEnumerator WaitForNextFrameAndEnableEscapeKey() {
        yield return null;

        mainMenu.escapeKeyDisabled = false;
    }

    // ------------------

    private void Awake() {
        mainMenu = GameObject.Find("UI").GetComponent<MainMenu>();
    }

    private void Update() {
        // If user press escape key and quit confirmation panel is active
        if(Input.GetKeyDown(KeyCode.Escape) && quitConfirmationActive) {
            // Deactivate quit confirmation panel
            OnQuitConfirmationCancelClick();
        }
    }
}
