using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewMeshBuilder : MonoBehaviour {
	public int pointCount = 20;
	[Range(0, 2)]
	public float padding = 0;
	[Range(0, 2 * Mathf.PI)]
	public float threshold = .1f;
	[Range(1, 50)]
	public float range = 5;

	private MeshFilter filter;

	// Start is called before the first frame update
	void Awake() {
		filter = GetComponent<MeshFilter>();
		filter.sharedMesh = new Mesh();
	}

	// Update is called once per frame
	void FixedUpdate() {
		UpdateMesh();
	}


	private Vector2[] GetUnitCircle(int count) {
		var points = new Vector2[count];
		float stepAngle = 2 * Mathf.PI / count;

		for (int i = 0; i < count; i++) {
			var angle = i * stepAngle;
			points[i] = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
		}

		return points;
	}

	private Vector2[] GetVisionPoints(Vector2[] unitCircle) {
		var points = new Vector2[unitCircle.Length];
		var centre = new Vector2(transform.position.x, transform.position.y);
		var distance = range;

		for (int i = 0; i < unitCircle.Length; i++) {
			var rayCast = Physics2D.RaycastAll(transform.position, unitCircle[i], distance);
			points[i] = rayCast.Length < 1 || rayCast[0].collider == null ? unitCircle[i] * distance : rayCast[0].point - centre;
			points[i] += points[i].normalized * padding;
		}

		return points;
	}

	private Vector2[] Simplify(Vector2[] points) {
		var newPoints = new List<Vector2>();
		var lastDir = (points[points.Length - 1] - points[0]).normalized;

		for (int i = 0; i < points.Length; i++) {
			var j = (i + 1) % points.Length;

			var dirToNext = (points[j] - points[i]).normalized;

			if (Vector2.Dot(dirToNext, lastDir) <= Mathf.Cos(threshold)) {
				newPoints.Add(points[i]);
				lastDir = dirToNext;
			}
		}

		return newPoints.ToArray();
	}

	/*private void OnDrawGizmos() {
		var points = Simplify(GetVisionPoints(GetUnitCircle(pointCount)));

		for (int i = 0; i < points.Length; i++) {
			int j = (i + 1) % points.Length;

			var point1 = new Vector3(points[i].x, points[i].y);
			var point2 = new Vector3(points[j].x, points[j].y);

			Gizmos.color = Color.Lerp(Color.red, Color.blue, (float)i / (points.Length - 1));
			Gizmos.DrawLine(point1 + transform.position, point2 + transform.position);
		}
	}*/

	private void UpdateMesh() {
		var points = Simplify(GetVisionPoints(GetUnitCircle(pointCount)));
		var triangulator = new Triangulator(points);

		var mesh = new Mesh();
		mesh.vertices = triangulator.GetPoints();
		mesh.triangles = triangulator.Triangulate();
		filter.sharedMesh = mesh;
	}
}
