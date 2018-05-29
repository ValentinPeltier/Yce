using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {
	[HideInInspector] public bool open = false;

    [System.NonSerialized] public bool escapeKeyDisabled = false;

    public enum PanelPosition {
        left, center, right
    }

    public class Panel {
        public GameObject gameObject = null;
        public Panel parent = null;
        public PanelPosition position = PanelPosition.right;
    }

    // --------------

    [Header("Cursor")]
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private int cursorSize;
    [SerializeField] private Color cursorColor;
    [SerializeField] private Vector2Int cursorShadowOffset;
    [SerializeField] private Color cursorShadowColor;

    [Header("Panel transition")]
    [SerializeField] private AnimationCurve transitionCurve;
    [SerializeField] private float transitionDuration;

    private Panel[] panels;
    private Transform panelsTransform;
	private Panel currentPanel = new Panel();

	private class Transition {
		public Panel panel = null;
		public Vector3 from;
        public Vector3 to;
	}

	private Transition[] transitions;
    private IEnumerator[] coroutines;

    private Vector2Int previousScreenSize;

	// ------------------
	
	/// <summary>
	/// Return panel gameObject based on panelName
	/// </summary>
	/// <param name="panelName">The name of the panel to get</param>
	public Panel GetPanel(string panelName) {
        GameObject panelGameObject = panelsTransform.Find(panelName).gameObject;
        PanelPosition panelPosition = PanelPosition.center;

        return new Panel {
            gameObject = panelGameObject,
            parent = null,
            position = panelPosition
		};
	}

	/// <summary>
	/// Open panel with slide transition
	/// </summary>
	public void OpenPanel(Panel panel) {
        // Stop transitions if any
		StopTransitions();

        // Call panel OnOpen
        panel.gameObject.GetComponent<PanelController>().OnOpen();
        
        // If no panel is open
        if (currentPanel.gameObject == null) {
			// Set open to true and activate canvas
			open = true;
			transform.Find("MenuCanvas").gameObject.SetActive(true);
		}
		else {
            // Set panel parent
            panel.parent = currentPanel;

            // Slide current panel from center to left
            transitions[0].panel = currentPanel;
            transitions[0].from  = currentPanel.gameObject.transform.position;
            transitions[0].to    = currentPanel.gameObject.transform.position - new Vector3(Screen.width, 0.0f, 0.0f);
            
            coroutines[0] = SlidePanelCoroutine(transitions[0]);
			StartCoroutine(coroutines[0]);

            // Slide panel from right to center
            transitions[1].panel = panel;
            transitions[1].from  = panel.gameObject.transform.position;
            transitions[1].to    = panel.gameObject.transform.position - new Vector3(Screen.width, 0.0f, 0.0f);

            coroutines[1] = SlidePanelCoroutine(transitions[1]);
            StartCoroutine(coroutines[1]);
            
            // Set panels position
            SetPanelPosition(currentPanel, PanelPosition.left);
            SetPanelPosition(panel, PanelPosition.center);
        }

        // Set currentPanel
        currentPanel = panel;
    }

	/// <summary>
	/// Close current panel with slide transition
	/// </summary>
	public void CloseCurrentPanel() {
        // Stop transitions if any
        StopTransitions();

        // If current panel has no parent
        if (currentPanel.parent == null) {
			// Close current panel without transition
			transform.Find("MenuCanvas").gameObject.SetActive(false);

            // No more currentPanel
            currentPanel.gameObject = null;

            // Set open to false
            open = false;
		}
		else {
            // Call current panel parent OnOpen
            currentPanel.parent.gameObject.GetComponent<PanelController>().OnOpen();

            // Slide current panel from center to right
            transitions[0].panel = currentPanel;
            transitions[0].from  = currentPanel.gameObject.transform.position;
            transitions[0].to    = currentPanel.gameObject.transform.position + new Vector3(Screen.width, 0.0f, 0.0f);

            coroutines[0] = SlidePanelCoroutine(transitions[0]);
            StartCoroutine(coroutines[0]);

            // Slide parent panel from left to center
            transitions[1].panel = currentPanel.parent;
            transitions[1].from  = currentPanel.parent.gameObject.transform.position;
            transitions[1].to    = currentPanel.parent.gameObject.transform.position + new Vector3(Screen.width, 0.0f, 0.0f);

            coroutines[1] = SlidePanelCoroutine(transitions[1]);
            StartCoroutine(coroutines[1]);

            // Set panels position
            SetPanelPosition(currentPanel, PanelPosition.right);
            SetPanelPosition(currentPanel.parent, PanelPosition.center);

            // Set currentPanel
            currentPanel = currentPanel.parent;
        }
	}

	// ------------------

    /// <summary>
    /// Recalculate panels position to adapt them to the screen
    /// </summary>
    private void ScreenSizeChanged() {
        // Stop transitions if any
        StopTransitions();

        // For each panel
        for(int i = 0, c = panels.Length; i < c; i++) {
            Panel panel = panels[i];

            // If panel is on left side
            if (panel.position == PanelPosition.left) {
                // Replace it on left side
                panel.gameObject.transform.localPosition = new Vector3(-Screen.width, 0.0f, 0.0f);
            }
            // If panel is on right side
            else if(panel.position == PanelPosition.right) {
                // Replace it on right side
                panel.gameObject.transform.localPosition = new Vector3(Screen.width, 0.0f, 0.0f);
            }
            // If panel is in center
            else {
                // Replace it in center
                panel.gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }
    }

    /// <summary>
    /// Set panel position
    /// </summary>
    /// <param name="panel">The panel to set its position</param>
    /// <param name="position">The position of the panel</param>
    private void SetPanelPosition(Panel panel, PanelPosition position) {
        for (int i = 0, c = panels.Length; i < c; i++) {
            // Panel found
            if (panels[i].gameObject == panel.gameObject) {
                // Change its position
                panels[i].position = position;
            }
        }
    }

    /// <summary>
    /// Reset panels position
    /// </summary>
    private void ResetPanelsPosition() {
        // Set first panel active and to center
        GameObject panel = panels[0].gameObject;

        panel.SetActive(true);
        panel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0.0f, 0.0f, 0.0f);

        // Set other panels active and to right
        for (int i = 1, c = panels.Length; i < c; i++) {
            panel = panels[i].gameObject;

            panel.SetActive(true);
            panel.GetComponent<RectTransform>().localPosition = new Vector3(Screen.width, 0.0f, 0.0f);
        }
    }

	/// <summary>
	/// Stop current transitions
	/// </summary>
	private void StopTransitions() {
        for (int i = 0, c = transitions.Length; i < c; i++) {
            // Check if transition exists
            if (transitions[i].panel == null) {
                return;
            }

            // Stop coroutine
            StopCoroutine(coroutines[i]);

            // Set transition panel position
            transitions[i].panel.gameObject.transform.position = transitions[i].to;

            // Set transition panel to active
            currentPanel = transitions[i].panel;

            // No more transition
            transitions[i].panel = null;
        }
	}

    /// <summary>
    /// Slide panel to destination
    /// </summary>
    /// <param name="transition">The transition to execute</param>
    private IEnumerator SlidePanelCoroutine(Transition transition) {
		for (float i = 0; i < transitionDuration; i += Time.deltaTime) {
			// Calculate time progression
			float time = i / transitionDuration;
            float progression = transitionCurve.Evaluate(time);

            // Calculate and set panel position
            //Vector3 np = transition.from + new Vector3(progression * Screen.width, 0.0f, 0.0f);
            Vector3 np = Vector3.Lerp(transition.from, transition.to, progression);
            transition.panel.gameObject.transform.position = np;

			// Wait for next frame
			yield return null;
		}

        // End of the transition
        // Reset transition value
		transition = new Transition();
	}

    // ------------------

    private void Awake() {
        // Get panels transform
        panelsTransform = transform.Find("MenuCanvas/Panels");

        // Set panels
        int c = panelsTransform.childCount;
        panels = new Panel[c];

        for (int i = 0; i < c; i++) {
            panels[i] = new Panel {
                gameObject = panelsTransform.GetChild(i).gameObject,
                position = (i == 0 ? PanelPosition.center : PanelPosition.right),
                parent = null
            };
        }
		
		// Set transitions and coroutines
		transitions = new Transition[2];
        transitions[0] = new Transition();
        transitions[1] = new Transition();
		coroutines = new IEnumerator[2];

        // Hide menu canvas
        transform.Find("MenuCanvas").gameObject.SetActive(false);

        // Set previousScreenSize to screen size
        previousScreenSize = new Vector2Int(Screen.width, Screen.height);

        // Reset panels position
        ResetPanelsPosition();

        // Initialize preferences
        transform.Find("MenuCanvas/Panels/Settings").GetComponent<SettingsPanelController>().InitializePreferences();
    }

    private void Update() {
        // If user clicks on escape and is not restricted by any panel
		if(!escapeKeyDisabled && Input.GetKeyDown(KeyCode.Escape)) {
            // If main menu is open
			if (open) {
				// Close current panel
				CloseCurrentPanel();
			}
			else {
				// Open main menu
				OpenPanel(GetPanel("Main"));
			}
		}

        // Detect if window size has changed
        Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
		
        if (previousScreenSize != screenSize) {
            // Call ScreenSizeChanged function
            ScreenSizeChanged();

            previousScreenSize = screenSize;
        }
	}

    private void OnGUI() {
		Cursor.visible = false;

		// Draw shadow first
		GUI.color = cursorShadowColor;
		GUI.DrawTexture(new Rect(Input.mousePosition.x + cursorShadowOffset.x, Screen.height - Input.mousePosition.y + cursorShadowOffset.y, cursorSize, cursorSize), cursorTexture);

		// Draw cursor above
		GUI.color = cursorColor;
		GUI.DrawTexture(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, cursorSize, cursorSize), cursorTexture);
	}
}
