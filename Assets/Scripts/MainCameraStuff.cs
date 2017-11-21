using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainCameraStuff : MonoBehaviour {

    public Text txt_pop_size;
    public Text txt_generation;

	public Vector3 MaxDistance = new Vector3(1400, 1400, 1400);

    private SimulationManager GOD;
	private Camera camera;
	private float currentDistance = 1000;

    // Use this for initialization
    void Start () {
		GOD = FindObjectOfType<SimulationManager>();
		camera = GetComponent<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (camera.enabled) {
			Vector3 center = Vector3.zero;
			float horizontal = Input.GetAxisRaw ("Horizontal");
			float vertical   = Input.GetAxisRaw ("Vertical");
			float scroll     = Input.GetAxisRaw ("Mouse ScrollWheel");

			currentDistance    = transform.position.magnitude;

			if (Input.GetKey (KeyCode.Space) && GOD.SelectedEntity != null ) {
				center = GOD.SelectedEntity.transform.position;
			}

			if (horizontal != 0) {
				transform.RotateAround (center, Vector3.up, Time.deltaTime * -20f * horizontal);
			}

			if (vertical != 0) {
				transform.RotateAround (center, Vector3.right, Time.deltaTime * 20f * vertical);
			}

			if (scroll != 0) {
				currentDistance -= scroll * Time.deltaTime * 12000f;
			}

			Vector3 currentPos = transform.position.normalized;

			transform.position = currentPos * currentDistance;
			transform.position = Vector3.ClampMagnitude (transform.position, 1400);

			transform.LookAt (center);
		}
    }

    private void OnGUI()
    {
		if( camera.enabled && GOD != null ) {}
    }
}
