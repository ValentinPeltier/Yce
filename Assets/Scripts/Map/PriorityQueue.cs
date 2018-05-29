using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue {
    public int Size { get { return nodes.Count; } }

    // --------------

	private List<Node> nodes = new List<Node>();

	// --------------

	private int Equals(Node a, Node b) {
		return a.F == b.F ? 0 : (a.F < b.F ? -1 : 1);
	}

	private void Siftdown(List<Node> nodes, int startIndex, int index) {
		Node node = nodes[index];

		int parentIndex;
		Node parent;

		while (index > startIndex) {
			parentIndex = (index - 1) >> 1;
			parent = nodes[parentIndex];

			if (Equals(node, parent) < 0) {
				nodes[index] = parent;
				index = parentIndex;
			}
			else {
				break;
			}
		}

		nodes[index] = node;
	}

	private void Siftup(List<Node> nodes, int index) {
		int startIndex = index;
		int endIndex = nodes.Count;
		int childIndex = 2 * index + 1;
		int rightIndex;

		Node newNode = nodes[index];

		while(childIndex < endIndex) {
			rightIndex = childIndex + 1;

			if(rightIndex < endIndex && !(Equals(nodes[childIndex], nodes[rightIndex]) < 0)) {
				childIndex = rightIndex;
			}

			nodes[index] = nodes[childIndex];
			index = childIndex;
			childIndex = 2 * index + 1;
		}

		nodes[index] = newNode;

		Siftdown(nodes, startIndex, index);
	}

	// --------------
	
	/// <summary>
	/// Find node in queue and update its g and h values
	/// </summary>
	/// <param name="node"></param>
	public void UpdateNode(Node node) {
		for(int i = 0, ic = nodes.Count; i < ic; i++) {
			if(nodes[i].Equals(node)) {
				nodes[i].g = node.g;
				nodes[i].h = node.h;

				return;
			}
		}
	}

	/// <summary>
	/// Add a node and reorder the queue
	/// </summary>
	public void Push(Node node) {
		nodes.Add(node);

		Siftdown(nodes, 0, nodes.Count - 1);
	}

	/// <summary>
	/// Return the highest priority node of the queue
	/// </summary>
	public Node Pop() {
		if (nodes.Count == 0) {
			return null;
		}

		Node lastNode, returnNode;

		// Pop last node from nodes list
		lastNode = nodes[nodes.Count - 1];
		nodes.RemoveAt(nodes.Count - 1);

		if(nodes.Count != 0) {
			returnNode = nodes[0];
			nodes[0] = lastNode;
			Siftup(nodes, 0);
		}
		else {
			returnNode = lastNode;
		}

		return returnNode;
	}
}