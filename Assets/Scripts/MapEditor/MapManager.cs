using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

	public MapData mapData;

	// Start is called before the first frame update
	void Start() {

		var spriteObject = new GameObject("Map");
		var renderer = spriteObject.AddComponent<SpriteRenderer>();

		renderer.sprite = Sprite.Create(
				mapData.LoadMapTexture(),
				mapData.GetRect(),
				Vector2.one * 0.5f,
				mapData.PixelsPerUnit
			);
	}

	// Update is called once per frame
	void Update() {

	}
}
