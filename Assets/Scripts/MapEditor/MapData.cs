using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class MapData {
	public string TexturePath;
	public Vector2Int TextureSize;
	public int PixelsPerUnit;
	public Vector2 Position;
	public byte[] CollisionData;

	public MapData() {

	}

	private MapData(string texturePath, Vector2Int textureSize, int pixelsPerUnit, Vector2 position, byte[] collisionData) {
		TexturePath = texturePath;
		TextureSize = textureSize;
		PixelsPerUnit = pixelsPerUnit;
		Position = position;
		CollisionData = collisionData;
	}

	public Vector2Int GridCellDimensions => TextureSize / PixelsPerUnit;

	public Texture2D LoadMapTexture() {
		var texture = new Texture2D(TextureSize.x, TextureSize.y);
		ImageConversion.LoadImage(texture, File.ReadAllBytes(TexturePath));

		return texture;
	}

	public Rect GetRect() => new Rect(Vector2.zero, TextureSize);

	public void SaveTo(string path) {
		var fileInfo = new FileInfo(path);

		if (fileInfo.Exists)
			fileInfo.Delete();

		using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write)) {
			var serialiser = new XmlSerializer(typeof(MapData));
			serialiser.Serialize(fileStream, this);
		}
	}

	public static MapData LoadFrom(string path) {
		try {
			var serialiser = new XmlSerializer(typeof(MapData));

			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
				return (MapData)serialiser.Deserialize(fileStream);
			}
		} catch (Exception e) {
			return new MapData("D:\\Users\\Kyle\\Downloads\\map_15x15.png", new Vector2Int(1050, 1050), 70, Vector2.one * .5f, null);
		}
	}
}
