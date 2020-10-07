using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskCameraController : MonoBehaviour {

	public static MaskCameraController Instance { get; private set; }
	
	public RenderTexture MaskTexture { get; private set; }

	private Camera camera;

	void Awake() {
		Instance = this;
		MaskTexture = new RenderTexture(Screen.width, Screen.height, -1);

		camera = GetComponent<Camera>();
		camera.targetTexture = MaskTexture;
	}
}
