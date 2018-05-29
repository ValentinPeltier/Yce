using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ScreenshotController : MonoBehaviour {

	[HideInInspector] public bool screenshotKeyDisabled = false;
	
	// -------------

    [SerializeField] private string screenshotFolderName;
    [SerializeField] private AudioClip audioClip;
	[SerializeField] private float animationDuration;

    private string screenshotFolderPath;
    private AudioSource audioSource;

	private GameObject screenshotCanvas;
	private Image overlay;
	
	private float animationStartTime = 0.0f;

    private Color baseOverlayColor;
	
    // --------------

    private string GetFilePath() {
        System.DateTime time = System.DateTime.Now;
        
        string baseFilePath = System.IO.Path.Combine(screenshotFolderPath, string.Format("screenshot_{0}-{1}-{2}_{3}-{4}", time.Year, time.Month, time.Day, time.Hour, time.Minute));
        string filePath = baseFilePath + ".png";

        // Increment file count while file exists
        for (int i = 0; System.IO.File.Exists(filePath); i++) {
            filePath = baseFilePath + (i == 0 ? "" : "_" + i) + ".png";
        }

        return filePath;
    }

    private IEnumerator TakeScreenshotCoroutine() {
        // Wait for the frame to be entirely rendered
        yield return new WaitForEndOfFrame();

        // Take screenshot
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
        texture.Apply();

        // Get file path
        string filePath = GetFilePath();

        // Write image on disk
        System.IO.File.WriteAllBytes(filePath, texture.EncodeToPNG());
		
		// Start borders animation
		screenshotCanvas.SetActive(true);
		animationStartTime = Time.time;
    }

    // --------------

    private void Awake() {
        // Get screenshot folder path
        screenshotFolderPath = System.IO.Path.Combine(Application.dataPath, screenshotFolderName);

        // If folder doesn't exists
        if (!Directory.Exists(screenshotFolderPath)) {
            // Create it
            Directory.CreateDirectory(screenshotFolderPath);
        }

        // Create audio component to play screenshot sound effect
        audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = audioClip;
        audioSource.bypassEffects = true;
        audioSource.bypassReverbZones = true;
        audioSource.playOnAwake = false;
		
		// Get screenshot canvas
		screenshotCanvas = GameObject.Find("UI").transform.Find("ScreenshotCanvas").gameObject;
        screenshotCanvas.SetActive(false);
		
		// Get overlay
		overlay = screenshotCanvas.transform.Find("Overlay").GetComponent<Image>();
        baseOverlayColor = overlay.color;
    }

    private void Update() {
		// Take screenshot
        if(!screenshotKeyDisabled && Input.GetKeyDown((KeyCode)PlayerPrefs.GetInt("Screenshot"))) {
            // Play screenshot sound effect
            audioSource.Play();
            
            // Take screenshot
            StartCoroutine(TakeScreenshotCoroutine());
        }
		
		// Screenshot animation
		if(animationStartTime != 0.0f) {
			// Get progression
            float progression = Mathf.Clamp01((Time.time - animationStartTime) / animationDuration);

            // If animation is in first half
            if (progression <= 0.5f) {
                progression = progression * 2;
            }
            else {
                progression = (1 - progression) * 2;
            }

            // Set overlay opacity
            overlay.color = new Color(baseOverlayColor.r, baseOverlayColor.g, baseOverlayColor.b, baseOverlayColor.a * progression);

            // End of animation
            if (progression >= 1.0f) {
				screenshotCanvas.SetActive(false);
				animationStartTime = 0.0f;
			}
		}
    }
}
