using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell {
	public Vector3 worldPosition;
	public Vector3Int localPosition;
	public bool walkable, isCell;

    public static Vector3 GetCellWorldPosition(Vector2Int map, Vector2Int cell) {
        // Get cells transform
        Transform cellsTransform = GameObject.Find("World/" + map.x + ";" + map.y + "/Cells").transform;

        // Transform point
        return cellsTransform.TransformPoint(new Vector3(cell.x, 0.0f, cell.y));
    }
}

public class Node {
	public Vector3 worldPosition;
	public Vector3Int localPosition;
    public bool walkable, isCell;

	public Node parent = null;

	public float g = 0.0f, h = 0.0f;
	public float F {
        get {
            return g + h;
        }
    }
	public bool opened = false, closed = false;

    // -------------

    public Node() {}

    public Node(Cell cell) {
        worldPosition = cell.worldPosition;
        localPosition = cell.localPosition;
        walkable = cell.walkable;
        isCell = cell.isCell;
    }

	public bool Equals(Node n) {
		return localPosition.x == n.localPosition.x && localPosition.z == n.localPosition.z;
	}
}

public class Grid {
	public enum Diagonal {
		Always,
		WhenNoObstacle,
		Never
	}

	public List<List<Node>> nodes;

	public int minX, minZ;

	// -----------------

	/// <summary>
	/// Determine if the node exists in the grid
	/// </summary>
	public bool NodeExistsAt(int x, int z) {
		if (x - minX >= 0 && x - minX < nodes.Count) {
			if (z - minZ >= 0 && z - minZ < nodes[x - minX].Count) {
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Determine if node at (x, z) is a cell
	/// </summary>
	private bool IsCell(int x, int z) {
		if (NodeExistsAt(x, z)) {
			return nodes[x - minX][z - minZ].isCell;
		}
		else {
			return false;
		}
	}

	/// <summary>
	/// Determine if node at (x, z) is walkable
	/// </summary>
	public bool IsWalkable(int x, int z) {
		return IsCell(x, z) && nodes[x - minX][z - minZ].walkable;
	}

	/// <summary>
	/// Return node at (x, z) position in the grid
	/// </summary>
	public Node GetNodeAt(int x, int z) {
		return IsCell(x, z) ? nodes[x - minX][z - minZ] : null;
	}

	/// <summary>
	/// Return all neighbors of specified node
	/// </summary>
	/// <param name="diagonal">Can the player walk in diagonal ?</param>
	public List<Node> GetNeighbors(Node node, Diagonal diagonal) {
		//        S              D
		//  +---+---+---+  +---+---+---+
		//  |   | 3 |   |  | 2 |   | 3 |
		//  +---+---+---+  +---+---+---+
		//  | 0 |   | 1 |  |   |   |   |
		//  +---+---+---+  +---+---+---+
		//  |   | 2 |   |  | 0 |   | 1 |
		//  +---+---+---+  +---+---+---+

		List< Node> neighbors = new List<Node>();
		int x = node.localPosition.x;
		int z = node.localPosition.z;

		bool s0 = false, s1 = false, s2 = false, s3 = false;

		// Straight
		if (IsWalkable(x - 1, z)) {
			neighbors.Add(GetNodeAt(x - 1, z));
			s0 = true;
		}
		if (IsWalkable(x + 1, z)) {
			neighbors.Add(GetNodeAt(x + 1, z));
			s1 = true;
		}
		if (IsWalkable(x, z - 1)) {
			neighbors.Add(GetNodeAt(x, z - 1));
			s2 = true;
		}
		if (IsWalkable(x, z + 1)) {
			neighbors.Add(GetNodeAt(x, z + 1));
			s3 = true;
		}

		// Diagonal
		if (diagonal != Diagonal.Never) {
			bool d0 = false, d1 = false, d2 = false, d3 = false;

			if(diagonal == Diagonal.WhenNoObstacle) {
				d0 = s0 && s2;
				d1 = s1 && s2;
				d2 = s0 && s3;
				d3 = s1 && s3;
			}
			else if(diagonal == Diagonal.Always) {
				d0 = d1 = d2 = d3 = true;
			}

			if (d0 && IsWalkable(x - 1, z - 1)) {
				neighbors.Add(GetNodeAt(x - 1, z - 1));
			}
			if (d1 && IsWalkable(x + 1, z - 1)) {
				neighbors.Add(GetNodeAt(x + 1, z - 1));
			}
			if (d2 && IsWalkable(x - 1, z + 1)) {
				neighbors.Add(GetNodeAt(x - 1, z + 1));
			}
			if (d3 && IsWalkable(x + 1, z + 1)) {
				neighbors.Add(GetNodeAt(x + 1, z + 1));
			}
		}

		return neighbors;
	}
}