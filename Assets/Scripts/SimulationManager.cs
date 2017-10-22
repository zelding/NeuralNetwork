using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public EntityController SelectedEntity { get; private set; }

    public Camera TopDownCamera;
    public Camera FollowingCamera;

    private Camera currentCamera;

    public int startingFishes = 30;
    public GameObject fishBody;

    private ThirdPersonCameraController thirdPersonCameraController;

    private bool isRunning = false;
    private int cycle      = 0;

    protected List<EntityController> Entities;

	// Use this for initialization
	void Start ()
    {
        TopDownCamera.enabled = true;
        FollowingCamera.enabled = false;
        currentCamera = TopDownCamera;

        cycle = 1;
        Entities = new List<EntityController>();
        thirdPersonCameraController = GetComponent<ThirdPersonCameraController>();
        thirdPersonCameraController.enabled = false;

		for(int i = 0; i < startingFishes; i++)
        {
            Vector3 startPosition = new Vector3(Random.Range(-100f, 100f), 0.85f, Random.Range(-100f, 100f));
            GameObject fish = Instantiate(fishBody, startPosition, Quaternion.identity);
            Entities.Add( fish.GetComponent<EntityController>() );
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        DetectMouseEvents();
        DetectKeyboardEvents();

		if ( isRunning )
        {

        }
	}

    private void OnGUI()
    {
        
    }

    private void DetectMouseEvents()
    {
        if( Input.GetMouseButtonDown(0) )
        {
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if( Physics.Raycast(ray, out hit) )
            {
                GameObject ClickedGameObject = hit.transform.root.gameObject;

                Debug.Log("Hit something");

                if (ClickedGameObject.tag == "Fish" )
                {
                    EntityController clickedController = ClickedGameObject.GetComponent<EntityController>();
                    Debug.Log("Found: " + clickedController.name);
                    SelectFish(clickedController);
                }
            }
        }
    }

    private void DetectKeyboardEvents()
    {
        if ( Input.GetKeyDown(KeyCode.F1) )
        {
            TopDownCamera.enabled = true;
            FollowingCamera.enabled = false;
            thirdPersonCameraController.enabled = false;
            currentCamera = TopDownCamera;
        }

        if ( Input.GetKeyDown(KeyCode.F2) )
        {
            TopDownCamera.enabled = false;
            FollowingCamera.enabled = true;
            thirdPersonCameraController.enabled = true;
            currentCamera = FollowingCamera;
        }
    }

    private void SelectFish(EntityController obj)
    {
        if ( obj == null || obj == SelectedEntity )
        {
            return;
        }

        SelectedEntity = obj;
        thirdPersonCameraController.Target      = SelectedEntity.transform;
        thirdPersonCameraController.turnSpeed   = SelectedEntity.Genes.Legs.turnSpeed;
        thirdPersonCameraController.smoothSpeed = SelectedEntity.Genes.Legs.smoothSpeed;
    }
}
