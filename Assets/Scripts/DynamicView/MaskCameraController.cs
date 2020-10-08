using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskCameraController : MonoBehaviour {

	public static MaskCameraController Instance { get; private set; }
	
	public RenderTexture MaskTexture { get; private set; }

	private Camera maskCamera;

	void Awake() {
		Instance = this;
		MaskTexture = new RenderTexture(Screen.width, Screen.height, -1);

		maskCamera = GetComponent<Camera>();
		maskCamera.targetTexture = MaskTexture;
	}
}
