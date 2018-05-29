using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {

	[HideInInspector] public Vector2Int currentMap;
	[HideInInspector] public Vector2Int currentCell;
    
    // ------------------


    [SerializeField] private float speed = 8.0f;

    private delegate IEnumerator FollowPathCoroutineDelegate();

    private MainMenu mainMenu;
    private CameraController cameraController;
    private Text mapCoordinates;

    private IEnumerator pathCoroutine = null;
	private bool isMoving = false;

    private struct Walk {
        public Vector3 from;
        public Vector3 to;
        public float duration;
        public float elapsedTime;
    }

    private Walk walk;

    private Vector2Int newMap, newCell;

    // --------------------

    /// <summary>
    /// Set player position, orientation and set camera position
    /// </summary>
    /// <param name="mapCoordinates">The coordinates of the map to set the player on</param>
    /// <param name="cellCoordinates">The coordinates of the cell to place the player on</param>
    /// <param name="orientation">The orientation of the player</param>
    /// <param name="cameraAnimation">Set camera position with animation ?</param>
    public void SetPosition(Vector2Int mapCoordinates, Vector2Int cellCoordinates, Vector3 orientation, bool cameraAnimation) {
        // Deactivate current map
        MapManager currentMapManager = MapManager.GetMapManager(currentMap);
        bool lineRenderState = currentMapManager.GetLineRenderState();

        currentMapManager.SetMapState(false);
        currentMapManager.SetLineRenderState(false);

        // Set new values for current map and cell
        currentMap = mapCoordinates;
        currentCell = cellCoordinates;

        // Set new map
        MapManager mapManager = MapManager.GetMapManager(mapCoordinates);
        mapManager.SetMapState(true);
        mapManager.SetLineRenderState(lineRenderState);

        // Get cell position
        Vector3 cellPosition = Cell.GetCellWorldPosition(mapCoordinates, cellCoordinates);

        // Place player on this cell
        transform.position = cellPosition + new Vector3(0.0f, transform.position.y, 0.0f);

        // Orientate player
        transform.rotation = new Quaternion {
            eulerAngles = orientation
        };
        
        // Set camera position
        cameraController.SetCameraPosition(new Vector3(31.2f * currentMap.x, 0.0f, 19.9f * currentMap.y), cameraAnimation);

        // Update UI map coordinates
        UpdateUIMapCoordinates();
    }

    /// <summary>
    /// Tell the player to follow path
    /// </summary>
    public void FollowPath(List<Cell> path) {
		if(speed == 0.0f) {
			Debug.LogError("Player speed value cannot be 0");
			return;
        }

        // If player is already following a path, stop here
        StopFollowPath();

        pathCoroutine = FollowPathCoroutine(path);
		StartCoroutine(pathCoroutine);
	}

    /// <summary>
    /// Follow path and change map
    /// </summary>
    /// <param name="path">The path to follow before changing map</param>
    /// <param name="map">The map to change</param>
    /// <param name="cell">The cell to place the player after changing map</param>
    public void ChangeMap(List<Cell> path, Vector2Int map, Vector2Int cell) {
        if (speed == 0.0f) {
            Debug.LogError("Player speed value cannot be 0");
            return;
        }

        // Set newMap and newMapCell
        newMap = map;
        newCell = cell;

        // If player is already following a path, stop here
        StopFollowPath();

        pathCoroutine = FollowPathCoroutine(path, ChangeMapCoroutine);
        StartCoroutine(pathCoroutine);
    }

    /// <summary>
    /// Tell the player to stop following its path
    /// </summary>
    public void StopFollowPath() {
		if (HasPath()) {
			StopCoroutine(pathCoroutine);
			pathCoroutine = null;
		}
	}

	/// <summary>
	/// Determine if player is already following a path
	/// </summary>
	public bool HasPath() {
		return pathCoroutine != null;
	}
    
	// ---------------------

    private void UpdateUIMapCoordinates() {
        mapCoordinates.text = "( " + currentMap.x + ", " + currentMap.y + " )";
    }

    private IEnumerator ChangeMapCoroutine() {
        // Wait until isMoving is false
        while(isMoving) {
            yield return null;
        }

        // Wait 0.05s
        yield return new WaitForSeconds(0.05f);

        // Calculate player orientation
        Vector2Int delta = newMap - currentMap;
        Vector3 orientation = new Vector3();

        if(delta.x != 0) {
            orientation = new Vector3(0.0f, 90.0f * delta.x, 0.0f);
        }

        if (delta.y != 0) {
            orientation = new Vector3(0.0f, delta.y > 0 ? 0.0f : 180.0f, 0.0f);
        }

        SetPosition(newMap, newCell, orientation, true);

        // Update UI map coordinates
        UpdateUIMapCoordinates();
    }

	private IEnumerator FollowPathCoroutine(List<Cell> path, FollowPathCoroutineDelegate d = null) {
        if (path != null) {
            int i = 1, c = path.Count;

            while (i < c) {
                if (!isMoving) {
                    Vector3 nextCell = path[i].worldPosition;

                    // Calculate orientation
                    Vector3 worldPosition = new Vector3(transform.position.x, 0.0f, transform.position.z);
                    Vector3 step = nextCell - worldPosition;

                    // Orientate player
                    transform.LookAt(nextCell + new Vector3(0.0f, transform.position.y, 0.0f));

                    float duration = step.magnitude / speed;

                    // Set walk
                    walk.from = transform.position;
                    walk.to = nextCell;
                    walk.duration = duration;
                    walk.elapsedTime = 0.0f;

                    isMoving = true;

                    currentCell = new Vector2Int(path[i].localPosition.x, path[i].localPosition.z);

                    i++;
                }

                yield return null;
            }

            pathCoroutine = null;

            if (d != null) {
                StartCoroutine(d());
            }
        }
	}

    // --------------------

    private void Awake() {
		mainMenu = GameObject.Find("UI").GetComponent<MainMenu>();
        cameraController = GameObject.Find("Camera").GetComponent<CameraController>();

        Transform gameCanvas = GameObject.Find("UI").transform.Find("GameCanvas");
        gameCanvas.gameObject.SetActive(true);

        mapCoordinates = gameCanvas.Find("MapCoordinates").GetComponent<Text>();
    }

    private void Update() {
        // If menu is open, do nothing
		if (mainMenu.open) {
			return;
		}

        if (isMoving) {
            // Update elapsedTime value
            walk.elapsedTime += Time.deltaTime;

			// Calculate movement progression
			float progression = Mathf.Clamp01(walk.elapsedTime / walk.duration);

            // Calculate new position
            // and don't care about y axis
            Vector3 np = Vector3.Lerp(walk.from - new Vector3(0.0f, walk.from.y, 0.0f), walk.to, progression);

            // Set player position
			transform.position = np + new Vector3(0.0f, transform.position.y, 0.0f);

            // End of movement
			if (progression == 1.0f) {
				isMoving = false;
			}
		}
	}
}
