using UnityEngine;
using System.Collections.Generic;


public class Triangulator {
	private List<Vector2> points;
	private int pointCount;

	public Triangulator(Vector2[] inputPoints) {
		points = new List<Vector2>(inputPoints);
		pointCount = inputPoints.Length;

		points.Add(Vector2.zero);
	}

	public Vector3[] GetPoints() {
		var pointsV3 = new Vector3[points.Count];

		for(int i = 0; i < points.Count; i++) {
			pointsV3[i] = new Vector3(points[i].x, points[i].y);
		}

		return pointsV3;
	}

	public int[] Triangulate() {
		var triangles = new List<int>();

		for (int i = 0; i < pointCount; i++) {
			int j = (i + 1) % pointCount;

			triangles.Add(i);
			triangles.Add(j);
			triangles.Add(points.Count - 1);
		}

		return triangles.ToArray();
	}
}