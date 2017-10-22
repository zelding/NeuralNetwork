using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public EntityController SelectedEntity { get; private set; }
    public EntityController HoveredEntity;

    public Camera TopDownCamera;
    public Camera FollowingCamera;

    private Camera currentCamera;

    public int startingFishes = 30;
    public GameObject fishBody;

    private ThirdPersonCameraController thirdPersonCameraController;

    private float spawnBoundary = 430f;
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
            Vector3 startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 0.85f, Random.Range(-spawnBoundary, spawnBoundary));

            do {
                startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 0.85f, Random.Range(-spawnBoundary, spawnBoundary));
                //startRotation = new Quaternion(0, Random.Range(-90f, 90f), 0, 0);
            } while (IsCloseToOthers(startPosition));

            GameObject fish = Instantiate(fishBody, startPosition, Quaternion.identity);

            var euler = fish.transform.eulerAngles;
            euler.y = Random.Range(-180f, 180f);
            fish.transform.eulerAngles = euler;

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
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject HoveredGameObject = hit.transform.root.gameObject;

            if (HoveredGameObject.tag == "Fish")
            {
                EntityController foundFish = HoveredGameObject.GetComponent<EntityController>();

                if (Input.GetMouseButtonDown(0))
                {
                    SelectFish(foundFish);
                }
                else
                {
                    HoverSelectable(foundFish);
                }
            }
        }
        else
        {
            ClearHover();
        }
    }

    private void DetectKeyboardEvents()
    {
        if ( Input.GetKeyDown(KeyCode.F1) )
        {
            ActivateTopDownCamera();
        }

        if ( Input.GetKeyDown(KeyCode.F2) )
        {
            ActivateFollowerCamera();
        }

        if ( Input.GetKeyDown(KeyCode.Escape) )
        {
            ActivateTopDownCamera();
            ClearSelection();
        }
    }

    private void SelectFish(EntityController obj)
    {
        if ( obj == null || obj == SelectedEntity )
        {
            return;
        }

        if ( SelectedEntity != null )
        {
            ClearSelection(true);
        }

        SelectedEntity = obj;
        thirdPersonCameraController.Target      = SelectedEntity.transform;
        thirdPersonCameraController.turnSpeed   = SelectedEntity.Genes.Legs.turnSpeed;
        thirdPersonCameraController.smoothSpeed = SelectedEntity.Genes.Legs.smoothSpeed;

        Renderer[] childRenderers = SelectedEntity.GetComponentsInChildren<Renderer>();

        foreach(Renderer r in childRenderers)
        {
            Material m = r.material;
            m.color = new Color(1, 0, 0, 1);
            r.material = m;
        }
    }

    private void HoverSelectable(EntityController obj)
    {
        if (obj == HoveredEntity || obj == SelectedEntity)
        {
            return;
        }

        if (obj == null)
        {
            ClearHover();
        }
        else
        {
            Renderer[] r = obj.GetComponentsInChildren<Renderer>();

            foreach (Renderer childRenderer in r)
            {
                Material m = childRenderer.material;
                m.color = Color.green;
                childRenderer.material = m;
            }

            HoveredEntity = obj;
        }
    }

    /// <summary>
    /// TODO: handle if the new and the old overlap
    /// </summary>
    private void ClearSelection(bool onlyResetColors = false)
    {
        if ( SelectedEntity == null )
        {
            return;
        }

        if (!onlyResetColors)
        {
            thirdPersonCameraController.enabled = false;
            thirdPersonCameraController.Target = null;
        }

        Renderer[] childRenderers = SelectedEntity.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in childRenderers)
        {
            Material m = r.material;
            m.color = Color.blue;
            r.material = m;
        }

        SelectedEntity = null;
    }

    private void ClearHover()
    {
        if ( HoveredEntity == null || HoveredEntity == SelectedEntity )
        {
            return;
        }

        Renderer[] r = HoveredEntity.GetComponentsInChildren<Renderer>();

        foreach (Renderer childRenderer in r)
        {
            Material m = childRenderer.material;
            m.color = Color.blue;
            childRenderer.material = m;
        }

        HoveredEntity = null;
    }

    private bool IsCloseToOthers(Vector3 pos)
    {
        if (Entities.Count == 0) return false;

        foreach(EntityController entity in Entities)
        {
            float min_x = entity.transform.position.x;
            float max_x = entity.transform.position.x + entity.transform.localScale.x;
            float min_z = entity.transform.position.z;
            float max_z = entity.transform.position.z + entity.transform.localScale.z;

            if ( pos.x > min_x && pos.x < max_x && pos.z > min_z && pos.z < max_z )
            {
                return true;
            }
        }

        return false;
    }

    private void ActivateTopDownCamera()
    {
        TopDownCamera.enabled = true;
        FollowingCamera.enabled = false;
        thirdPersonCameraController.enabled = false;
        currentCamera = TopDownCamera;
    }

    private void ActivateFollowerCamera()
    {
        TopDownCamera.enabled = false;
        FollowingCamera.enabled = true;
        thirdPersonCameraController.enabled = true;
        currentCamera = FollowingCamera;
    }

}
