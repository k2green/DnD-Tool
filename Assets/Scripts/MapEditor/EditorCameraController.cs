using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class EditorCameraController : MonoBehaviour {

	public float minZoom = 1f;
	public float maxZoom = 10f;
	public float scrollSpeed = 0.2f;

	private Camera cameraComponent;

	void Start() {
		cameraComponent = GetComponent<Camera>();
	}

	// Update is called once per frame
	void Update() {
		Scroll(scrollSpeed * Time.deltaTime * Input.mouseScrollDelta.y);

		var mov = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * cameraComponent.orthographicSize * Time.deltaTime;
		transform.Translate(mov);
	}

	private void Scroll(float value) {
		cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize - value, minZoom, maxZoom);
	}
}
