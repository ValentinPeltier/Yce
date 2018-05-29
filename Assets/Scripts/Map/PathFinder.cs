using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder {
	/// <summary>
	/// Return world position path based on node parent
	/// </summary>
	/// <param name="node">Last node of the path</param>
	private static List<Cell> RetrievePath(Node node) {
		List<Cell> path = new List<Cell> {
			new Cell {
				localPosition = node.localPosition,
				worldPosition = node.worldPosition
			}
		};

		while (node.parent != null) {
			node = node.parent;
			path.Add(new Cell {
				localPosition = node.localPosition,
				worldPosition = node.worldPosition
			});
		}

		path.Reverse();

		return path;
	}
	
	/// <summary>
	/// Reset all nodes g, h, opened and closed values
	/// </summary>
	/// <param name="grid"></param>
	private static void ResetVariables(Grid grid) {
		// For each node in grid
		for (int x = 0, xc = grid.nodes.Count; x < xc; x++) {
			for(int z = 0, zc = grid.nodes[x].Count; z < zc; z++) {
				Node node = grid.nodes[x][z];

				// Reset its values
				node.parent = null;
				node.g = 0.0f;
				node.h = 0.0f;
				node.opened = false;
				node.closed = false;
			}
		}
	}

	/// <summary>
	/// Return path from start node to end node using A* algorithm
	/// </summary>
	/// <param name="grid">The grid in which the path will be searched</param>
	/// <param name="diagonal">Can the player move diagonally ?</param>
	public static List<Cell> FindPath(Grid grid, int startX, int startZ, int endX, int endZ, Grid.Diagonal diagonal) {
		PriorityQueue openList = new PriorityQueue();
		Node startNode = grid.GetNodeAt(startX, startZ);
		Node endNode = grid.GetNodeAt(endX, endZ);

		startNode.g = 0;
		startNode.h = 0;

		// Push start node to list
		openList.Push(startNode);
		startNode.opened = true;

		// While open list is not empty
		while (openList.Size > 0) {
			// Get node with minimal f value
			Node node = openList.Pop();

			// If openList was void, then initialize node coordinates
			if(node == null) {
				node.localPosition.x = startX;
				node.localPosition.z = startZ;
			}

			node.closed = true;

			// End node reached
			if(node.Equals(endNode)) {
				List<Cell> path = RetrievePath(node);

				// Reset variables
				ResetVariables(grid);

				// Return worldPosition path
				return path;
			}

			// For each neighbor
			List<Node> neighbors = grid.GetNeighbors(node, diagonal);

			for (int i = 0, c = neighbors.Count; i < c; i++) {
				Node neighbor = neighbors[i];

				if(neighbor.closed) {
					continue;
				}

				int x = neighbor.localPosition.x;
				int z = neighbor.localPosition.z;

				float ng = node.g + ((node.localPosition.x - x == 0 || node.localPosition.z - z == 0) ? 1 : Mathf.Sqrt(2));

				// If neighbor has not been already tested or cannot be reached by shorter path
				if(!neighbor.opened || ng < neighbor.g) {
					neighbor.g = ng;
					neighbor.h = Mathf.Abs(x - endX) + Mathf.Abs(z - endZ);
					neighbor.parent = node;

					if(!neighbor.opened) {
						openList.Push(neighbor);
						neighbor.opened = true;
					}
					else {
						// Neighbor can be reached by shorter path
						openList.UpdateNode(neighbor);
					}
				}
			}
		}

		// No path found
		ResetVariables(grid);
		return null;
	}
}