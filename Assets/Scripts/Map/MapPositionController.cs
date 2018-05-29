using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapPositionController : MonoBehaviour {
	
	private void Update() {
		Vector2Int mapCoordinates = MapManager.GetMapCoordinates(name);
		transform.position = new Vector3(31.2f * mapCoordinates.x, 0.0f, 19.9f * mapCoordinates.y);
	}
}
