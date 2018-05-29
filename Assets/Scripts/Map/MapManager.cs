using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapManager : MonoBehaviour {

	public Vector2Int[] occupiedCells;
	
	// ------------------
	
	private bool isLoaded = false;
	private bool debugEnabled;

	private MainMenu mainMenu;

    private World world;
	private GameObject map;
	private PlayerMovement playerMovement;

	private List<Cell> cells = new List<Cell>();
	private Grid mapGrid = new Grid();
	private int cellsCount = 0;

    private GameObject cellPrefab;

	private float mapPadding = Mathf.Sqrt(2.0f) / 2.0f;
	private Vector3 mapMin, mapMax, mapMinPadding, mapMaxPadding;
    private Vector3Int cellMin = new Vector3Int(), cellMax = new Vector3Int();

    private bool mapState = false;
    private bool lineRenderState = false;

    // ------------------
	
	/// <summary>
    /// Parse "x;z" format map name into Vector2Int
    /// </summary>
    /// <param name="mapName">The name of the map to parse</param>
    public static Vector2Int GetMapCoordinates(string mapName) {
        string[] stringCoordinates = mapName.Split(';');
		
		int xCoordinates, yCoordinates;
		if(!(int.TryParse(stringCoordinates[0], out xCoordinates) && int.TryParse(stringCoordinates[1], out yCoordinates))) {
			return new Vector2Int();
		}
		
        return new Vector2Int(xCoordinates, yCoordinates);
    }
	
    /// <summary>
    /// Return the map manager component associated to the map
    /// </summary>
    /// <param name="mapCoordinates">The coordinates of the map</param>
    public static MapManager GetMapManager(Vector2Int mapCoordinates) {
        return GameObject.Find("World/" + mapCoordinates.x + ";" + mapCoordinates.y).GetComponent<MapManager>();
    }

    /// <summary>
    /// Set the map state
    /// </summary>
    /// <param name="state">Is this map the current map ?</param>
    public void SetMapState(bool state) {
        // If mapState is already at state, do nothing
        if(state == mapState) {
            return;
        }

        // Set mapState to state
        mapState = state;

        // Set map color
        Material mapMaterial = transform.Find("Map").GetComponent<Renderer>().material;

        if (state) {
            mapMaterial.SetColor("_Color", new Color(mapMaterial.color.r / world.mapStateFactor, mapMaterial.color.g / world.mapStateFactor, mapMaterial.color.b / world.mapStateFactor, mapMaterial.color.a));
        }
        else {
            mapMaterial.SetColor("_Color", new Color(mapMaterial.color.r * world.mapStateFactor, mapMaterial.color.g * world.mapStateFactor, mapMaterial.color.b * world.mapStateFactor, mapMaterial.color.a));
            SetLineRenderState(false);
        }
    }

    /// <summary>
    /// Get the map state
    /// </summary>
    public bool GetMapState() {
        return mapState;
    }

    /// <summary>
    /// Set line render state associated to this map
    /// </summary>
    /// <param name="state">The line render state</param>
    public void SetLineRenderState(bool state) {
        lineRenderState = state;

        Transform cells = transform.Find("Cells");

        // If lineRenderer is already at state, do nothing
        if (cells.GetChild(0).GetComponent<LineRenderer>().enabled == state) {
            return;
        }

        // For each cell
        for (int i = 0, c = cells.childCount; i < c; i++) {
            // Set its line renderer state
            cells.GetChild(i).GetComponent<LineRenderer>().enabled = state;
        }
    }

    /// <summary>
    /// Get line render state associated to this map
    /// </summary>
    /// <returns></returns>
    public bool GetLineRenderState() {
        return lineRenderState;
    }

    /// <summary>
    /// Get boundaries of map
    /// </summary>
    /// <param name="mapMin">The minimum point of the map (bottom left corner)</param>
    /// <param name="mapMax">The maximum point of the map (top right corner)</param>
    public void GetMapBoundaries(out Vector3 mapMin, out Vector3 mapMax) {
        mapMin = this.mapMin;
        mapMax = this.mapMax;
    }

    // ------------------

    /// <summary>
    /// Load map to be useable
    /// </summary>
    private void LoadMap() {
        if (isLoaded) {
            return;
        }

        // Calculate map position
        CalculateMapBoundaries();

        GameObject cellsGameObject = Instantiate(world.cellsPrefab, transform);
        cellsGameObject.name = "Cells";

        // Create cells from map
        CreateCell(0, 0);

        // Create grid from cells
        CreateGrid();

        // Set map grid obstacles
        SetObstacles();

        isLoaded = true;
    }

    /// <summary>
    /// Calculate map boundaries with and whitout padding
    /// </summary>
    private void CalculateMapBoundaries() {
		Transform mt = map.transform;

        mapMin = new Vector3(mt.position.x - mt.lossyScale.x * 5, 0f, mt.position.z - mt.lossyScale.z * 5);
        mapMax = new Vector3(mt.position.x + mt.lossyScale.x * 5, 0f, mt.position.z + mt.lossyScale.z * 5);

        mapMinPadding = new Vector3(mt.position.x - mt.lossyScale.x * 5 + mapPadding, 0f, mt.position.z - mt.lossyScale.z * 5 + mapPadding);
		mapMaxPadding = new Vector3(mt.position.x + mt.lossyScale.x * 5 - mapPadding, 0f, mt.position.z + mt.lossyScale.z * 5 - mapPadding);
	}

	/// <summary>
	/// Check if a cell already exists in cell array at (x, z)
	/// </summary>
	private bool CellExists(Vector3Int cell) {
		for (int i = 0, c = cells.Count; i < c; i++) {
			if (cells[i].localPosition == cell) {
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Instantiate cell at (x, z), add it to the cells and instantiate all neighbors
	/// </summary>
	private void CreateCell(int x, int z) {
        // Calculate cell world position
        Vector3 cp = transform.Find("Cells").TransformPoint(new Vector3(x, 0.0f, z));
        Vector3Int clp = new Vector3Int(x, 0, z);

        // If new cell is not in map coordinates or if cell already exists
        bool isInside = cp.x > mapMinPadding.x && cp.x < mapMaxPadding.x && cp.z > mapMinPadding.z && cp.z < mapMaxPadding.z;

        if (!isInside || CellExists(clp)) {
            return;
        }

        // Instantiate cell gameObject, set its position and rename it
        GameObject cell = Instantiate(cellPrefab, transform.Find("Cells"));
		cell.transform.localPosition += new Vector3(x, 0.0f, z);
		cell.name = (++cellsCount).ToString();

        // Hide cell if debug is disabled
        if (!debugEnabled) {
            cell.GetComponent<Renderer>().enabled = false;
        }

		// Update minCell and maxCell
		if (clp.x < cellMin.x) {
			cellMin.x = clp.x;
        }
		if (clp.z < cellMin.z) {
			cellMin.z = clp.z;
		}
		if (clp.x > cellMax.x) {
			cellMax.x = clp.x;
		}
		if (clp.z > cellMax.z) {
			cellMax.z = clp.z;
		}

        // Add cell to cells
        Cell c = new Cell {
            localPosition = clp,
            worldPosition = cp,
            walkable = true,
            isCell = true
		};

		cells.Add(c);

		// Recursive call for neighbors cells
		CreateCell(x + 1, z);
		CreateCell(x - 1, z);
		CreateCell(x, z + 1);
		CreateCell(x, z - 1);
	}

	/// <summary>
	/// Create grid from instantiated cells
	/// </summary>
	private void CreateGrid() {
		List<List<Node>> nodes = new List<List<Node>>();

		// Create 2x2 matrix of nodes
		for (int x = 0, xc = cellMax.x - cellMin.x + 1; x < xc; x++) {
			nodes.Add(new List<Node>());

			for (int z = 0, zc = cellMax.z - cellMin.z + 1; z < zc; z++) {
				nodes[x].Add(new Node { isCell = false, walkable = false });
			}
		}

		// Set nodes values
		for (int i = 0, c = cells.Count; i < c; i++) {
			int x = cells[i].localPosition.x - cellMin.x;
			int z = cells[i].localPosition.z - cellMin.z;

			nodes[x][z] = new Node(cells[i]);
        }

		mapGrid.nodes = nodes;
		mapGrid.minX = cellMin.x;
		mapGrid.minZ = cellMin.z;
	}

	/// <summary>
	/// Set map grid obstacles
	/// </summary>
	private void SetObstacles() {		
        // For each occupied cell
		for(int i = 0, c = occupiedCells.Length; i < c; i++) {
			int x = occupiedCells[i].x - cellMin.x;
			int z = occupiedCells[i].y - cellMin.z;

            // Set its walkable value to false
			mapGrid.nodes[x][z].walkable = false;
		}
	}

    /// <summary>
    /// Calculate nearest cell on current map and nearest cell on new map
    /// Return true if no error, false otherwise
    /// </summary>
    /// <param name="hitMap">The map the user click on</param>
    /// <param name="hitPoint">The point the user click on</param>
    /// <param name="nearestCellOnCurrentMap">The nearest cell on current map</param>
    /// <param name="nearestCellOnNewMap">The nearest cell on new map</param>
    private bool GetNearestCells(GameObject hitMap, Vector3 hitPoint, out Vector2Int nearestCellOnCurrentMap, out Vector2Int nearestCellOnNewMap) {
        Vector2Int map = GetMapCoordinates(hitMap.name);

        // Calculate map difference
        int dx = map.x - playerMovement.currentMap.x;
        int dz = map.y - playerMovement.currentMap.y;

        /* -------------- */
        /* --- X axis --- */
        /* -------------- */
        if(dx != 0) {
            // Only select cells
            int layerMask = 1 << 8;

            RaycastHit[] hitInfos0 = Physics.RaycastAll(hitPoint, new Vector3(-dx, 0.0f, 0.0f), 31.2f, layerMask);

            // For each raycast hit
            foreach(RaycastHit hitInfo0 in hitInfos0) {
                // If collider is not on current map, continue
                if(hitInfo0.collider.transform.parent.parent.name != playerMovement.currentMap.x + ";" + playerMovement.currentMap.y) {
                    continue;
                }

                // Set nearestCellOnCurrentMap
                Transform ncocmTransform = hitInfo0.collider.transform;
                nearestCellOnCurrentMap = new Vector2Int((int)ncocmTransform.localPosition.x, (int)ncocmTransform.localPosition.z);

                List<RaycastHit> raycastHits = new List<RaycastHit>();

                RaycastHit[] hitInfos1 = Physics.RaycastAll(ncocmTransform.position, new Vector3(dx, 0.0f, 0.0f), 31.2f, layerMask);

                foreach (RaycastHit hitInfo1 in hitInfos1) {
                    // If collider is not on new map, continue
                    if(hitInfo1.collider.transform.parent.parent.name != name) {
                        continue;
                    }

                    // If collider is on new map, add it to the list
                    raycastHits.Add(hitInfo1);
                }

                // Order the list
                raycastHits = raycastHits.OrderBy(hitInfo => hitInfo.collider.transform.position.x).ToList();

                // Set the value of nearestCellOnMap
                Vector3 lp = new Vector3();

                if (dx > 0) {
                    lp = raycastHits[0].collider.transform.localPosition;
                }
                else if (dx < 0) {
                    lp = raycastHits[raycastHits.Count-1].collider.transform.localPosition;
                }

                nearestCellOnNewMap = new Vector2Int((int)lp.x, (int)lp.z);
                return true;
            }
        }

        /* -------------- */
        /* --- Z axis --- */
        /* -------------- */
        if (dz != 0) {
            // Only select cells
            int layerMask = 1 << 8;

            RaycastHit[] hitInfos0 = Physics.RaycastAll(hitPoint, new Vector3(0.0f, 0.0f, -dz), 19.9f, layerMask);

            // For each raycast hit
            foreach (RaycastHit hitInfo0 in hitInfos0) {
                // If collider is not on current map, continue
                if (hitInfo0.collider.transform.parent.parent.name != playerMovement.currentMap.x + ";" + playerMovement.currentMap.y) {
                    continue;
                }

                // Set nearestCellOnCurrentMap
                Transform ncocmTransform = hitInfo0.collider.transform;
                nearestCellOnCurrentMap = new Vector2Int((int)ncocmTransform.localPosition.x, (int)ncocmTransform.localPosition.z);

                List<RaycastHit> raycastHits = new List<RaycastHit>();

                RaycastHit[] hitInfos1 = Physics.RaycastAll(ncocmTransform.position, new Vector3(0.0f, 0.0f, dz), 19.9f, layerMask);

                foreach (RaycastHit hitInfo1 in hitInfos1) {
                    // If collider is not on new map, continue
                    if (hitInfo1.collider.transform.parent.parent.name != name) {
                        continue;
                    }

                    // If collider is on new map, add it to the list
                    raycastHits.Add(hitInfo1);
                }

                // Order the list
                raycastHits = raycastHits.OrderBy(hitInfo => hitInfo.collider.transform.position.z).ToList();

                // Set the value of nearestCellOnMap
                Vector3 lp = new Vector3();

                if (dz > 0) {
                    lp = raycastHits[0].collider.transform.localPosition;
                }
                else if (dz < 0) {
                    lp = raycastHits[raycastHits.Count - 1].collider.transform.localPosition;
                }

                nearestCellOnNewMap = new Vector2Int((int)lp.x, (int)lp.z);
                return true;
            }
        }

        nearestCellOnCurrentMap = new Vector2Int();
        nearestCellOnNewMap = new Vector2Int();
        return false;
    }

    // ------------------

    private void Awake() {
        // Initialization
		mainMenu = GameObject.Find("UI").GetComponent<MainMenu>();
		world = GameObject.Find("World").GetComponent<World>();
		map = transform.Find("Map").gameObject;

		cellPrefab = world.cellPrefab;
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();

        // Get debugEnabled value
        debugEnabled = transform.parent.GetComponent<MapDebug>().debugEnabled;

        // Set map color to dark
        Material mapMaterial = transform.Find("Map").GetComponent<Renderer>().material;
        mapMaterial.SetColor("_Color", new Color(mapMaterial.color.r * world.mapStateFactor, mapMaterial.color.g * world.mapStateFactor, mapMaterial.color.b * world.mapStateFactor, mapMaterial.color.a));

        // Load map
        LoadMap();
    }

    private void Update() {
        // If the main menu is open, do nothing
		if(mainMenu.open) {
			return;
		}

        RaycastHit hitInfo = new RaycastHit();

        // If user clicks
        if (Input.GetMouseButtonDown(0)) {
            // Check if user clicks on this map
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Only select maps
            int layerMask = 1 << 10;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask) && hitInfo.collider != null) {
                // If user doesn't click on this map, quit
                if (hitInfo.collider.gameObject != transform.Find("Map").gameObject) {
                    return;
                }
            }
            else {
                // No collider, quit
                return;
            }


            // If map is not the current map (-> Change map)
            if (!mapState) {
                // Calculate the coordinates of the nearest cells
                Vector2Int nearestCellOnCurrentMap, nearestCellOnMap;
                if (GetNearestCells(hitInfo.collider.transform.parent.gameObject, hitInfo.point, out nearestCellOnCurrentMap, out nearestCellOnMap)) {
                    // Tell player to go to this map by the nearest cell on current map
                    List<Cell> path = PathFinder.FindPath(GetMapManager(playerMovement.currentMap).mapGrid, playerMovement.currentCell.x, playerMovement.currentCell.y, nearestCellOnCurrentMap.x, nearestCellOnCurrentMap.y, Grid.Diagonal.WhenNoObstacle);

                    playerMovement.ChangeMap(path, GetMapCoordinates(name), nearestCellOnMap);
                }

                return;
            }
            

            // If user clicks on cells (-> Move)
            layerMask = 1 << 8;

			if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask) && hitInfo.collider != null) {
                if(hitInfo.collider.transform.parent.parent.gameObject != gameObject) {
                    return;
                }

				Vector3 colliderLocalPosition = hitInfo.collider.gameObject.transform.localPosition;

				Vector2Int playerCell = playerMovement.currentCell;
				Vector3Int endPos = new Vector3Int((int)colliderLocalPosition.x, 0, (int)colliderLocalPosition.z);

				// If destination cell is walkable
				if (mapGrid.GetNodeAt(endPos.x, endPos.z).walkable) {
					// Calculate path
					List<Cell> path = PathFinder.FindPath(mapGrid, playerCell.x, playerCell.y, endPos.x, endPos.z, Grid.Diagonal.WhenNoObstacle);

					// ====== DEBUG ======
					if(debugEnabled) {
                        // Reset cells color
                        for(int x = 0, xc = mapGrid.nodes.Count; x < xc; x++) {
							for (int z = 0, zc = mapGrid.nodes[x].Count; z < zc; z++) {
								if (Physics.Raycast(mapGrid.nodes[x][z].worldPosition + new Vector3(0.0f, 1.0f, 0.0f), Vector3.down, out hitInfo, 2.0f, layerMask) && hitInfo.collider != null) {
                                    // Reset cell color
                                    hitInfo.collider.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 1.0f, 1.0f));
                                }
							}
						}

                        // Set occupied cells to red
                        Transform cellsTransform = transform.Find("Cells");
	
						for(int i = 0, c = occupiedCells.Length; i < c; i++) {
                            if (Physics.Raycast(cellsTransform.TransformPoint(occupiedCells[i].x, 0.0f, occupiedCells[i].y) + new Vector3(0.0f, 1.0f, 0.0f), Vector3.down, out hitInfo, 2.0f, layerMask) && hitInfo.collider != null) {
                                hitInfo.collider.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.75f, 0.0f, 0.0f));
                            }
						}

						// Set blue color for path
						for(int i = 0, c = path.Count; i < c; i++) {
							if(Physics.Raycast(path[i].worldPosition + new Vector3(0.0f, 0.5f, 0.0f), Vector3.down, out hitInfo, 2.0f, layerMask) && hitInfo.collider != null) {
								hitInfo.collider.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.0f, 0.0f, 1.0f - (float)(i + 1) / c));
							}
						}
					}
					// ====== END DEBUG ======

                    // If a path has been found
					if (path != null) {
                        // Tell the player to follow the path
                        playerMovement.FollowPath(path);
					}
				}
			}
		}
		else if(debugEnabled) {
			// Activate occupied cells and set their color to red
			Transform cellsTransform = transform.Find("Cells");

			for(int i = 0, c = occupiedCells.Length; i < c; i++) {
                int layerMask = 1 << 8;

                if (Physics.Raycast(cellsTransform.TransformPoint(occupiedCells[i].x, 0.0f, occupiedCells[i].y) + new Vector3(0.0f, 1.0f, 0.0f), Vector3.down, out hitInfo, 2.0f, layerMask) && hitInfo.collider != null) {
                    hitInfo.collider.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.7f, 0.0f, 0.0f));
                }
            }
		}
	}
}