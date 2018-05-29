using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class ButtonSoundEffect : MonoBehaviour {

    private void Awake() {
        ButtonAudioManager buttonAudioManager = GameObject.Find("UI").GetComponent<ButtonAudioManager>();

        // Add button OnClick listener
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate {
            buttonAudioManager.PlayAudio();
        });
    }
}
