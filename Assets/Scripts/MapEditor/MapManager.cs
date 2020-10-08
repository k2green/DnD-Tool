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
	private GameObject mapObject;

	public Vector2Int MouseGridPos { get; private set; }


	// Start is called before the first frame update
	void Start() {
		mapObject = CreateMapObject();

		grid = new PhysicsGrid(8, mapData.GridCellDimensions, mapData.Position);
		mainCamera = Camera.main;
	}

	// Update is called once per frame
	void Update() {
		var mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		var local = mapObject.transform.InverseTransformPoint(mousePos);
		var offset = new Vector2(local.x, local.y) + (Vector2)grid.UnitDimensions * 0.5f;
		var scaled = offset * grid.CellDivisions;

		MouseGridPos = new Vector2Int(Mathf.FloorToInt(scaled.x), Mathf.FloorToInt(scaled.y));

		if (Input.GetMouseButton(0) && grid.IsInRange(MouseGridPos)) {
			grid.SetHasCollider(MouseGridPos.x, MouseGridPos.y, true);
		}
	}

	private void OnRenderObject() {
		grid.Render(mapObject.transform, MouseGridPos, gridlineMainColor, gridlineSubColor, collisionColor, selectionColor);
	}

	private GameObject CreateMapObject() {
		var spriteObject = new GameObject("Map");
		var renderer = spriteObject.AddComponent<SpriteRenderer>();

		spriteObject.transform.position = new Vector3(mapData.Position.x, mapData.Position.y);
		renderer.sprite = Sprite.Create(
				mapData.LoadMapTexture(),
				mapData.GetRect(),
				Vector2.one * 0.5f,
				mapData.PixelsPerUnit
			);
		spriteObject.isStatic = true;

		return spriteObject;
	}
}
