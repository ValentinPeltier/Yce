using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField] private float animationDuration;
    [SerializeField] private AnimationCurve animationCurve;

    private Vector3 basePosition;
    private Vector3 basePositionOnMap;

    private float animationStartTime;
    private Vector3 startPosition, destPosition;

    private PlayerController playerController;

    // --------------

    public Vector3 GetBasePositionOnMap() {
        return basePositionOnMap;
    }

    public void SetCameraPosition(Vector3 position, bool animation = true) {
        // Stop replace camera animation
        playerController.StopReplaceCameraAnimation();

        // If no animation
        if(animation == false) {
            transform.position = basePosition + position;
            basePositionOnMap = basePosition + position;
            return;
        }

        // Launch animation
        animationStartTime = Time.time;

        startPosition = transform.position;
        destPosition = basePosition + position;

        basePositionOnMap = destPosition;
    }

    // --------------

    private void Awake() {
        basePosition = basePositionOnMap = transform.position;

        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    private void Update() {
        // If animation
        if(animationStartTime != 0.0f) {
            // Calculate progression
            float progression = Mathf.Clamp01((Time.time - animationStartTime) / animationDuration);
            progression = Mathf.Clamp01(animationCurve.Evaluate(progression));

            // Calculate camera position
            Vector3 position = Vector3.Lerp(startPosition, destPosition, progression);

            // Set camera position
            transform.position = position;

            // If end of animation
            if(progression == 1.0f) {
                animationStartTime = 0.0f;
            }
        }
    }
}
