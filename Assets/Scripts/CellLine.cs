using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CellLine : MonoBehaviour {
    private void Start() {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        Transform cells = transform.parent;

        Vector3 position = transform.position - transform.parent.parent.position;

        lineRenderer.SetPosition(0, position + cells.TransformPoint(new Vector3(-0.5f, 0.0f, -0.5f)));
        lineRenderer.SetPosition(1, position + cells.TransformPoint(new Vector3(-0.5f, 0.0f, 0.5f)));
        lineRenderer.SetPosition(2, position + cells.TransformPoint(new Vector3(0.5f, 0.0f, 0.5f)));
        lineRenderer.SetPosition(3, position + cells.TransformPoint(new Vector3(0.5f, 0.0f, -0.5f)));
        lineRenderer.SetPosition(4, position + cells.TransformPoint(new Vector3(-0.5f, 0.0f, -0.5f)));
        lineRenderer.SetPosition(5, position + cells.TransformPoint(new Vector3(-0.5f, 0.0f, 0.5f)));
    }

}
