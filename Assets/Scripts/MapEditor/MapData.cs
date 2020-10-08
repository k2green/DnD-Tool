using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class MapData {
	public string TexturePath;
	public Vector2Int TextureSize;
	public int PixelsPerUnit;
	public Vector2 Position;

	public Vector2Int GridCellDimensions => TextureSize / PixelsPerUnit;

	public Texture2D LoadMapTexture() {
		var texture = new Texture2D(TextureSize.x, TextureSize.y);
		ImageConversion.LoadImage(texture, File.ReadAllBytes(TexturePath));

		return texture;
	}

	public Rect GetRect() => new Rect(Vector2.zero, TextureSize);
}
