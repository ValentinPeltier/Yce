using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
[RequireComponent(typeof(World))]
public class MapDebug : MonoBehaviour {

	public bool debugEnabled = true;
	
    // ----------------

	private MapManager[] mapManagers;
	private Vector2Int[][] occupiedCells;

    // ----------------

    private void Update() {
#if DEBUG
        if(!debugEnabled) {
            return;
        }

        int childCount = transform.childCount;

        // Get map managers
        mapManagers = new MapManager[childCount];
        occupiedCells = new Vector2Int[childCount][];

        for(int i = 0; i < childCount; i++) {
            mapManagers[i] = transform.GetChild(i).GetComponent<MapManager>();
            occupiedCells[i] = mapManagers[i].occupiedCells;
        }

        // For each map
        for (int i = 0; i < mapManagers.Length; i++) {
            // Get cells transform to convert local positions to world positions
            Transform cellsTransform = GetComponent<World>().cellsPrefab.transform;

            // For each occupied cell
            for (int j = 0, c = occupiedCells[i].Length; j < c; j++) {
                // Get its world position
                Vector3 cellWorldPosition = mapManagers[i].transform.position + cellsTransform.TransformPoint(new Vector3(occupiedCells[i][j].x, 0.0f, occupiedCells[i][j].y));

                // Draw ray
                Debug.DrawRay(cellWorldPosition + new Vector3(0.0f, 1.0f, 0.0f), Vector3.down, Color.red, 0.0f);
            }
        }
#endif
	}
}
