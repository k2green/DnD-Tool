using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

	public MapData mapData;
	public Color gridlineMainColor = Color.black;
	public Color gridlineSubColor = Color.black;
	public Color selectionColor = Color.red;
	public Color collisionColor = Color.blue;

	private PhysicsGrid grid;
	private Camera mainCamera;
	private SpriteRenderer mapRenderer;

	public Vector2Int MouseGridPos { get; private set; }


	// Start is called before the first frame update
	void Start() {
		mapRenderer = CreateMapObject();
		UpdateMapSprite();

		grid = new PhysicsGrid(8, mapData.GridCellDimensions, mapData.Position);
		mainCamera = Camera.main;
	}

	// Update is called once per frame
	void Update() {
		var mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		var local = mapRenderer.transform.InverseTransformPoint(mousePos);
		var offset = new Vector2(local.x, local.y) + (Vector2)grid.UnitDimensions * 0.5f;
		var scaled = offset * grid.CellDivisions;

		MouseGridPos = new Vector2Int(Mathf.FloorToInt(scaled.x), Mathf.FloorToInt(scaled.y));

		if (Input.GetMouseButton(0) && grid.IsInRange(MouseGridPos)) {
			grid.SetHasCollider(MouseGridPos.x, MouseGridPos.y, true);
		}
	}

	private void OnRenderObject() {
		var start = -(Vector2)grid.UnitDimensions * 0.5f;
		var end = (Vector2)grid.UnitDimensions * 0.5f;

		PhysicsGrid.LineMaterial.SetPass(0);

		GL.PushMatrix();
		// Set transformation matrix for drawing to
		// match our transform
		GL.MultMatrix(mapRenderer.transform.localToWorldMatrix);

		grid.RenderCells(start, MouseGridPos, MouseGridPos, collisionColor, selectionColor);
		grid.RenderLines(start, end, gridlineMainColor, gridlineSubColor);

		GL.PopMatrix();
	}

	private SpriteRenderer CreateMapObject() {
		var spriteObject = new GameObject("Map");
		var renderer = spriteObject.AddComponent<SpriteRenderer>();

		spriteObject.transform.position = new Vector3(mapData.Position.x, mapData.Position.y);
		spriteObject.isStatic = true;

		return renderer;
	}

	public void UpdateMapSprite() => mapRenderer.sprite = Sprite.Create(
				mapData.LoadMapTexture(),
				mapData.GetRect(),
				Vector2.one * 0.5f,
				mapData.PixelsPerUnit
			);
}
