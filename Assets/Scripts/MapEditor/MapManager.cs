using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

	public string mapPath;
	public Camera mainCamera;
	public Color gridlineMainColor = Color.black;
	public Color gridlineSubColor = Color.black;
	public Color collisionColor = Color.red;
	public Color highlightColor1 = Color.blue;
	public Color highlightColor2 = Color.green;

	private MapData mapData;
	private PhysicsGrid grid;
	private SpriteRenderer mapRenderer;
	private bool fillMode;

	public Vector2Int MouseGridPosA { get; private set; }
	public Vector2Int MouseGridPosB { get; private set; }

	private void Awake() {
		mapData = MapData.LoadFrom(mapPath);
	}

	// Start is called before the first frame update
	void Start() {
		mapRenderer = CreateMapObject();
		UpdateMapSprite();

		grid = new PhysicsGrid(8, mapData.GridCellDimensions, mapData.Position, mapData.CollisionData);
		mainCamera = Camera.main;

		MouseGridPosA = -Vector2Int.one;
		MouseGridPosB = -Vector2Int.one;

		fillMode = true;
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetMouseButtonDown(0))
			MouseGridPosA = GetMouseGridCoords();

		if (Input.GetMouseButton(0))
			MouseGridPosB = GetMouseGridCoords();

		if (Input.GetMouseButtonUp(0)) {
			grid.FillRange(MouseGridPosA, MouseGridPosB, fillMode);

			MouseGridPosA = -Vector2Int.one;
			MouseGridPosB = -Vector2Int.one;
		}

		if (Input.GetKeyDown(KeyCode.C))
			fillMode = !fillMode;

		if (Input.GetKeyDown(KeyCode.F))
			grid.FloodFill(GetMouseGridCoords());
	}

	private Vector2Int GetMouseGridCoords() {
		if (mainCamera == null || mapRenderer == null)
			return Vector2Int.zero;

		var mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		var local = mapRenderer.transform.InverseTransformPoint(mousePos);
		var offset = new Vector2(local.x, local.y) + (Vector2)grid.UnitDimensions * 0.5f;
		var scaled = offset * grid.CellDivisions;

		return new Vector2Int(Mathf.FloorToInt(scaled.x), Mathf.FloorToInt(scaled.y));
	}

	private void OnRenderObject() {
		var currentMousePos = GetMouseGridCoords();
		var start = -(Vector2)grid.UnitDimensions * 0.5f;
		var end = (Vector2)grid.UnitDimensions * 0.5f;

		PhysicsGrid.LineMaterial.SetPass(0);

		GL.PushMatrix();
		// Set transformation matrix for drawing to
		// match our transform
		GL.MultMatrix(mapRenderer.transform.localToWorldMatrix);

		if (fillMode) {
			grid.RenderHighlightCells(start, MouseGridPosA, MouseGridPosB, currentMousePos, highlightColor1, highlightColor2);
			grid.RenderColliderCells(start, collisionColor);
		} else {
			grid.RenderColliderCells(start, collisionColor);
			grid.RenderHighlightCells(start, MouseGridPosA, MouseGridPosB, currentMousePos, highlightColor1, highlightColor2);
		}

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

	private void OnDisable() {
		mapData.CollisionData = grid.CollisionData;
		mapData.SaveTo(mapPath);
	}

	public void UpdateMapSprite() => mapRenderer.sprite = Sprite.Create(
				mapData.LoadMapTexture(),
				mapData.GetRect(),
				Vector2.one * 0.5f,
				mapData.PixelsPerUnit
			);
}
