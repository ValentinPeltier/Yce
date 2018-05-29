using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    
    [SerializeField] private float[] zoomValues;
    [SerializeField] private float zoomMoveSpeed = 1.0f;
    [SerializeField] private float zoomAnimationDuration;
    [SerializeField] private float replaceCameraAnimationDuration;
    [SerializeField] private Vector2 moveZoomPadding;

    private PlayerMovement playerMovement;
    private new Camera camera;
    private MainMenu mainMenu;
    
    private int zoomIndex = 0;
    private Vector3 mousePrevPosition;

    private float zoomAnimationStartTime = 0.0f;
    private float zoomAnimationStartValue, zoomAnimationEndValue;

    private float replaceCameraAnimationStartTime = 0.0f;
    private Vector3 replaceCameraAnimationStartValue, replaceCameraAnimationEndValue;

    // ---------------

    public void StopReplaceCameraAnimation() {
        replaceCameraAnimationStartTime = 0.0f;
    }

    // ---------------

    private bool IsOnMap(Vector3 lookPoint) {
        // Get map bounds
        Vector3 mapMin, mapMax;
        MapManager.GetMapManager(playerMovement.currentMap).GetMapBoundaries(out mapMin, out mapMax);

        if(lookPoint.x >= mapMin.x + moveZoomPadding.x && lookPoint.z >= mapMin.z + moveZoomPadding.y && lookPoint.x <= mapMax.x - moveZoomPadding.x && lookPoint.z <= mapMax.z - moveZoomPadding.y) {
            return true;
        }

        return false;
    }

    private bool IsOnMapX(float lookPointX) {
        // Get map bounds
        Vector3 mapMin, mapMax;
        MapManager.GetMapManager(playerMovement.currentMap).GetMapBoundaries(out mapMin, out mapMax);

        if (lookPointX >= mapMin.x + moveZoomPadding.x && lookPointX <= mapMax.x - moveZoomPadding.x) {
            return true;
        }

        return false;
    }

    private bool IsOnMapZ(float lookPointZ) {
        // Get map bounds
        Vector3 mapMin, mapMax;
        MapManager.GetMapManager(playerMovement.currentMap).GetMapBoundaries(out mapMin, out mapMax);

        if (lookPointZ >= mapMin.z + moveZoomPadding.y && lookPointZ <= mapMax.z - moveZoomPadding.y) {
            return true;
        }

        return false;
    }

    // ---------------

    private void Awake() {
        playerMovement = GetComponent<PlayerMovement>();
        camera = GameObject.Find("Camera").GetComponent<Camera>();
        mainMenu = GameObject.Find("UI").GetComponent<MainMenu>();

        mousePrevPosition = Input.mousePosition;
    }

    private void Update () {
        // If main menu is open, do nothing
        if (mainMenu.open) {
            return;
        }

        // Display cells
        if (Input.GetKeyDown((KeyCode)PlayerPrefs.GetInt("DisplayCells"))) {
            // Toggle line render state for current map
            MapManager map = MapManager.GetMapManager(playerMovement.currentMap);
            map.SetLineRenderState(!map.GetLineRenderState());
        }

        // Zoom
        if(Input.mouseScrollDelta != new Vector2()) {
            if (Input.mouseScrollDelta.y > 0.0f) {
                // Zoom in
                zoomIndex++;

                if(zoomIndex >= zoomValues.Length) {
                    zoomIndex = zoomValues.Length - 1;
                }
            }
            else {
                // Zoom out
                zoomIndex--;

                if (zoomIndex < 0) {
                    zoomIndex = 0;
                }
            }

            zoomAnimationStartTime = Time.time;
            zoomAnimationStartValue = camera.fieldOfView;
            zoomAnimationEndValue = zoomValues[zoomIndex];

            // Replace camera if no zoom
            if (zoomIndex == 0) {
                replaceCameraAnimationStartTime = Time.time;
                replaceCameraAnimationStartValue = camera.transform.position;
                replaceCameraAnimationEndValue = camera.GetComponent<CameraController>().GetBasePositionOnMap();
            }
        }

        // Zoom animation
        if(zoomAnimationStartTime != 0.0f) {
            float progression = Mathf.Clamp01((Time.time - zoomAnimationStartTime) / zoomAnimationDuration);

            float fov = zoomAnimationStartValue + (zoomAnimationEndValue - zoomAnimationStartValue) * progression;
            camera.fieldOfView = fov;

            // End of animation
            if(progression == 1.0f) {
                zoomAnimationStartTime = 0.0f;
            }
        }

        // Replace camera animation
        if (replaceCameraAnimationStartTime != 0.0f) {
            float progression = Mathf.Clamp01((Time.time - replaceCameraAnimationStartTime) / replaceCameraAnimationDuration);

            Vector3 position = replaceCameraAnimationStartValue + (replaceCameraAnimationEndValue - replaceCameraAnimationStartValue) * progression;
            camera.GetComponent<Transform>().position = position;

            // End of animation
            if (progression == 1.0f) {
                replaceCameraAnimationStartTime = 0.0f;
            }
        }

        // Move camera while zooming
        if (zoomIndex > 0 && Input.GetMouseButton(2)) {
            Vector3 mouseDeltaPos = Input.mousePosition - mousePrevPosition;
            Vector3 translation = new Vector3(-mouseDeltaPos.x * zoomMoveSpeed, 0.0f, -mouseDeltaPos.y * zoomMoveSpeed);

            // Get camera look point
            Vector3 cameraLookPoint = new Vector3();
            int layerMask = 1 << 10;
            RaycastHit hitInfo;

            if(Physics.Raycast(camera.transform.position, camera.transform.forward, out hitInfo, Mathf.Infinity, layerMask) && hitInfo.collider != null) {
                cameraLookPoint = hitInfo.point;
            }

            // Check for camera position
            if (IsOnMap(cameraLookPoint + translation)) {
                camera.GetComponent<Transform>().position += translation;
            }
            else if(IsOnMapX(cameraLookPoint.x + translation.x)) {
                camera.GetComponent<Transform>().position += new Vector3(translation.x, 0.0f, 0.0f);
            }
            else if (IsOnMapZ(cameraLookPoint.z + translation.z)) {
                camera.GetComponent<Transform>().position += new Vector3(0.0f, 0.0f, translation.z);
            }
        }

        mousePrevPosition = Input.mousePosition;
    }
}
