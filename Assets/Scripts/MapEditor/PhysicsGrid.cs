using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsGrid {

	public int CellDivisions { get; }

	public Vector2Int UnitDimensions { get; }
	private Vector2 position;

	public float CellSize => 1f / CellDivisions;
	public Vector2Int CellCounts => UnitDimensions * CellDivisions;

	public int CellCount => CellCounts.x * CellCounts.y;
	private int byteCount => Mathf.CeilToInt(CellCount / 8f);

	private byte[] collisionData;
	public byte[] CollisionData => collisionData;

	public PhysicsGrid(int cellDivisions, Vector2Int unitDimensions, Vector2 position) {
		this.CellDivisions = cellDivisions;
		this.UnitDimensions = unitDimensions;
		this.position = position;

		collisionData = new byte[byteCount];
	}

	public bool IsInRange(Vector2Int pos) => pos.x >= 0 && pos.x < CellCounts.x && pos.y >= 0 && pos.y < CellCounts.y;


	public bool HasCollider(int x, int y) => HasCollider(y * CellCounts.x + x);
	public bool HasCollider(int index) {
		var byteIndex = index / 8;
		var shiftCount = 7 - (index % 8);
		var mask = 1;

		if (shiftCount > 0)
			mask = mask << shiftCount;

		return (collisionData[byteIndex] & mask) != 0;
	}

	public void SetHasCollider(int x, int y, bool value) => SetHasCollider(y * CellCounts.x + x, value);
	public void SetHasCollider(int index, bool value) {
		var byteIndex = index / 8;
		var shiftCount = 7 - (index % 8);
		var mask = 1;
		var valueNum = value ? 1 : 0;

		if (shiftCount > 0) {
			mask = mask << shiftCount;
			valueNum = valueNum << shiftCount;
		}

		mask = 255 - mask;

		collisionData[byteIndex] = (byte)((collisionData[byteIndex] & mask) + valueNum);
	}

	private static Material lineMaterial;
	private static Material LineMaterial {
		get {
			if (lineMaterial == null)
				CreateLineMaterial();

			return lineMaterial;
		}
	}


	private static void CreateLineMaterial() {
		if (!lineMaterial) {
			Shader shader = Shader.Find("Hidden/Internal-Colored");
			lineMaterial = new Material(shader);
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;

			// Turn on alpha blending
			lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			// Turn backface culling off
			lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			// Turn off depth writes
			lineMaterial.SetInt("_ZWrite", 0);
		}
	}

	public void Render(Transform transform, Vector2Int selected, Color lineColor1, Color lineColor2, Color collisionColor, Color selectionColor) {
		var start = -(Vector2)UnitDimensions * 0.5f;
		var end = (Vector2)UnitDimensions * 0.5f;

		CreateLineMaterial();
		// Apply the line material
		LineMaterial.SetPass(0);

		GL.PushMatrix();
		// Set transformation matrix for drawing to
		// match our transform
		GL.MultMatrix(transform.localToWorldMatrix);

		GL.Begin(GL.QUADS);

		for (int y = 0; y < CellCounts.y; y++) {
			for (int x = 0; x < CellCounts.x; x++) {
				if (x == selected.x && y == selected.y) {
					GL.Color(selectionColor);
				} else if (HasCollider(x, y)) {
					GL.Color(collisionColor);
				} else {
					continue;
				}

				var position = start + new Vector2(x, y) * CellSize;
				GL.Vertex3(position.x, position.y, 0);
				GL.Vertex3(position.x + CellSize, position.y, 0);
				GL.Vertex3(position.x + CellSize, position.y + CellSize, 0);
				GL.Vertex3(position.x, position.y + CellSize, 0);
			}
		}

		GL.End();

		// Draw lines
		GL.Begin(GL.LINES);
		for (int x = 0; x <= CellCounts.x; x++) {
			GL.Color(x % CellDivisions == 0 ? lineColor1 : lineColor2);
			GL.Vertex3(start.x + x * CellSize, start.y, 0);
			GL.Vertex3(start.x + x * CellSize, end.y, 0);
		}

		for (int y = 0; y <= CellCounts.y; y++) {
			GL.Color(y % CellDivisions == 0 ? lineColor1 : lineColor2);
			GL.Vertex3(start.x, start.y + y * CellSize, 0);
			GL.Vertex3(end.x, start.y + y * CellSize, 0);
		}
		GL.End();
		GL.PopMatrix();
	}
}
