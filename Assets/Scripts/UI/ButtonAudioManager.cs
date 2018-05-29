using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAudioManager : MonoBehaviour {
    
    [SerializeField] private AudioClip audioClip;
    [SerializeField] [Range(0.0f, 1.0f)] private float audioVolume;

    private AudioSource audioSource;

    // --------------

    /// <summary>
    /// Play button audio clip
    /// </summary>
    public void PlayAudio() {
        audioSource.Play();
    }

    // --------------

    private void Awake() {
        // Create audio component to play button sound effect
        audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = audioClip;
        audioSource.volume = audioVolume;
        audioSource.bypassEffects = true;
        audioSource.bypassReverbZones = true;
        audioSource.playOnAwake = false;
    }
}
