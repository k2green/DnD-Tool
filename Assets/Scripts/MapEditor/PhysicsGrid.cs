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

	public PhysicsGrid(int cellDivisions, Vector2Int unitDimensions, Vector2 position, byte[] data) {
		this.CellDivisions = cellDivisions;
		this.UnitDimensions = unitDimensions;
		this.position = position;

		if (data != null && data.Length == byteCount)
			collisionData = data;
		else
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
	public static Material LineMaterial {
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

	public static int Min(int a, int b) {
		if (a < b)
			return a;
		else
			return b;
	}

	public static int Max(int a, int b) {
		if (a < b)
			return b;
		else
			return a;
	}

	public static (Vector2Int, Vector2Int) GetBounds(Vector2Int a, Vector2Int b) {
		var min = new Vector2Int(Min(a.x, b.x), Min(a.y, b.y));
		var max = new Vector2Int(Max(a.x, b.x), Max(a.y, b.y));

		return (min, max);
	}

	private (Vector2Int, Vector2Int) RescaleBounds((Vector2Int, Vector2Int) ab) {
		var (a, b) = ab;
		var min = new Vector2Int(Max(a.x, 0), Max(a.y, 0));
		var max = new Vector2Int(Min(CellCounts.x - 1, b.x), Min(CellCounts.y - 1, b.y));

		return (min, max);
	}

	public void FillRange(Vector2Int a, Vector2Int b, bool value = true) {
		(a, b) = RescaleBounds(GetBounds(a, b));

		for (int y = a.y; y <= b.y; y++) {
			for (int x = a.x; x <= b.x; x++) {
				SetHasCollider(x, y, value);
			}
		}
	}

	public void RenderColliderCells(Vector2 start, Color collisionColor) {
		GL.Begin(GL.QUADS);

		for (int y = 0; y < CellCounts.y; y++) {
			for (int x = 0; x < CellCounts.x; x++) {
				if (HasCollider(x, y))
					GL.Color(collisionColor);
				else
					continue;

				DrawCell(start + new Vector2(x, y) * CellSize);
			}
		}

		GL.End();
	}

	public void RenderHighlightCells(Vector2 start, Vector2Int a, Vector2Int b, Vector2Int mousePos, Color selectionColor1, Color selectionColor2) {
		GL.Begin(GL.QUADS);
		(a, b) = RescaleBounds(GetBounds(a, b));

		for (int y = a.y; y <= b.y; y++) {
			for (int x = a.x; x <= b.x; x++) {
				GL.Color(selectionColor1);

				DrawCell(start + new Vector2(x, y) * CellSize);
			}
		}

		if (IsInRange(mousePos)) {
			GL.Color(selectionColor2);
			DrawCell(start + (Vector2)mousePos * CellSize);
		}

		GL.End();
	}

	public void FloodFill(Vector2Int pos) {
		if (!IsInRange(pos) || HasCollider(pos.x, pos.y))
			return;

		SetHasCollider(pos.x, pos.y, true);

		FloodFill(pos + new Vector2Int(1, 0));
		FloodFill(pos + new Vector2Int(0, 1));
		FloodFill(pos + new Vector2Int(-1, 0));
		FloodFill(pos + new Vector2Int(0, -1));
	}

	private void DrawCell(Vector2 position) {
		GL.Vertex3(position.x, position.y, 0);
		GL.Vertex3(position.x + CellSize, position.y, 0);
		GL.Vertex3(position.x + CellSize, position.y + CellSize, 0);
		GL.Vertex3(position.x, position.y + CellSize, 0);
	}

	public void RenderLines(Vector2 start, Vector2 end, Color lineColor1, Color lineColor2) {
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
	}
}
