using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour
{
    public EntityController SelectedEntity { get; private set; }
    public EntityController HoveredEntity;

    public Camera TopDownCamera;
    public Camera FollowingCamera;

    public Text txt_generation;
    public Text txt_entity_name;
    public Text txt_entity_energy;
    public Text txt_entity_age;

    public Text txt_input;
    public Text txt_output;

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
            Vector3 startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));

            do {
                startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));
                //startRotation = new Quaternion(0, Random.Range(-90f, 90f), 0, 0);
            } while (IsCloseToOthers(startPosition));

            GameObject fish = Instantiate(fishBody, startPosition, Quaternion.identity);

            var euler = fish.transform.eulerAngles;
            euler.y = Random.Range(-180f, 180f);
            fish.transform.eulerAngles = euler;

            EntityController fishController = fish.GetComponent<EntityController>();
            Entities.Add(fishController); 
        }

        isRunning = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
        DetectMouseEvents();
        DetectKeyboardEvents();

        if ( isRunning)
        {

        }
        else
        {
            if ( Entities.Count > 0 )
            {
                CreateChildrenEntities();
            }
        }
	}

    private void FixedUpdate()
    {
        if (isRunning)
        {
            bool theyAreAllDead = false;

            foreach (EntityController entity in Entities)
            {
                if (entity.isAlive())
                {
                    return;
                }
            }

            isRunning = theyAreAllDead;
        }
    }

    private void CreateChildrenEntities()
    {
        Debug.Log("New gen start");
        cycle++;

        Entities.Sort();
        Entities.Reverse();

        List<EntityController> nextGeneration = new List<EntityController>();

        for(int j = 0; j< 2; j++)
        for(int i = 0; i < Entities.Count / 2; i++)
        {
            Vector3 startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));

            do
            {
                startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));
                //startRotation = new Quaternion(0, Random.Range(-90f, 90f), 0, 0);
            } while (IsCloseToOthers(startPosition));

            GameObject fish = Instantiate(fishBody, startPosition, Quaternion.identity);

            var euler = fish.transform.eulerAngles;
            euler.y = Random.Range(-180f, 180f);
            fish.transform.eulerAngles = euler;

            EntityController fishController = fish.GetComponent<EntityController>();
            fishController.InheritFrom(Entities[i]);

            nextGeneration.Add(fishController);
        }

        Entities = nextGeneration;

        isRunning = true;
    }

    public static string FloatFToString(float f)
    {
        return f.ToString("0.00");
    }

    public static string JoinArray(string[] data)
    {
        return string.Join(", ", data);
    }

    private void OnGUI()
    {
        txt_generation.text = "Generation: " + cycle;

        if ( SelectedEntity != null )
        {
            txt_entity_name.text = "Entity name: " + SelectedEntity.Name;
            txt_entity_energy.text = "Energy: " + SelectedEntity.Energy;
            txt_entity_age.text = "Age: " + SelectedEntity.Age;

            txt_input.text = "Input: " + JoinArray(System.Array.ConvertAll(SelectedEntity.Input, new System.Converter<float, string>(FloatFToString)));
            txt_output.text = "Output: " + JoinArray(System.Array.ConvertAll(SelectedEntity.Output, new System.Converter<float, string>(FloatFToString)));
        }
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
            else
            {
                ClearHover();
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
