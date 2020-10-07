using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour {

	private Camera mainCamera;
	private Material material;

	public Shader shader;

	// Start is called before the first frame update
	void Start() {
		mainCamera = GetComponent<Camera>();
		material = new Material(shader);

		material.SetTexture("_MaskTex", MaskCameraController.Instance.MaskTexture);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination) {
		Graphics.Blit(source, destination, material);
	}
}
