﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour
{
    public EntityController SelectedEntity { get; private set; }
    public EntityController HoveredEntity;

    public EntityInfoRenderer EntityInfoRenderer;

    public Camera TopDownCamera;
    public Camera FollowingCamera;

    public Text txt_pop_size;
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
    private bool isRunning = true;
    private int cycle      = 0;

    protected List<EntityController> Entities;
    protected List<EntityController> WorstEntities;
    protected List<EntityController> BestEntities;
    protected List<EntityController> MiddleEntities;

    // Use this for initialization
    void Start ()
    {
        TopDownCamera.enabled = true;
        FollowingCamera.enabled = false;
        currentCamera = TopDownCamera;

        cycle = 1;
        Entities = new List<EntityController>();
        WorstEntities = new List<EntityController>();
        BestEntities = new List<EntityController>();
        MiddleEntities = new List<EntityController>();

        thirdPersonCameraController = GetComponent<ThirdPersonCameraController>();
        thirdPersonCameraController.enabled = false;

		for(int i = 0; i < startingFishes; i++)
        {
            var startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));

            do {
                startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));
                //startRotation = new Quaternion(0, Random.Range(-90f, 90f), 0, 0);
            } while (IsCloseToOthers(startPosition));

            GameObject fish = Instantiate(fishBody, startPosition, Quaternion.identity);

            Vector3 euler = fish.transform.eulerAngles;
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
	}

    private void LateUpdate()
    {
        if (!isRunning)
        {
            if (Entities.Count > 0)
            {
                CreateChildrenEntities();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isRunning && Entities.Count > 0)
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

        int count = Entities.Count;
        int middle = (Entities.Count % 2 == 0) ? Entities.Count / 2 : (Entities.Count + 1) / 2;

        var nextGeneration = new List<EntityController>();

        WorstEntities.Add(Entities[ 0 ]);
        BestEntities.Add(Entities[ Entities.Count - 1 ]);
        MiddleEntities.Add(Entities[ middle ]);

        for(int i = middle; i < count; i++)
        {
            for( int k = 0; k < 2; k++ )
            {
                var startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));

                do
                {
                    startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));
                } while( IsCloseToOthers(startPosition) );

                GameObject fish = Instantiate(fishBody, startPosition, Quaternion.identity);

                Vector3 euler = fish.transform.eulerAngles;
                euler.y = Random.Range(-180f, 180f);
                fish.transform.eulerAngles = euler;

                EntityController fishController = fish.GetComponent<EntityController>();
                fishController.InheritFrom(Entities[ i ]);

                nextGeneration.Add(fishController);
            }
        }

        foreach( EntityController entity in Entities )
        {
            Destroy(entity.gameObject);
            //Entities.Remove(entity);
        }

        Entities = nextGeneration;

        isRunning = true;
    }

    private void OnGUI()
    {
        txt_pop_size.text   = "Population: " + WorstEntities.Count + " / " + Entities.Count;
        txt_generation.text = "Generation: " + cycle;

        if ( SelectedEntity != null && EntityInfoRenderer.SelectedEntity == null ) {
            EntityInfoRenderer.SelectedEntity = SelectedEntity;
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

            if (SelectedEntity.isAlive())
            {
                m.color = Color.red;
            }
            else
            {
                m.color = new Color(0.67f, 0, 0, 1);
            }

            r.material = m;
        }

        EntityInfoRenderer.SelectedEntity = SelectedEntity;
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

                if (obj.isAlive())
                {
                    m.color = Color.green;
                }
                else
                {
                    m.color = new Color(0, 0.67f, 0, 1);
                }

                m.color = Color.green;
                childRenderer.material = m;
            }

            HoveredEntity = obj;
        }

        EntityInfoRenderer.SelectedEntity = HoveredEntity;
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
            if (SelectedEntity.isAlive())
            {
                m.color = new Color(0, 0, 1, 1);
            }
            else
            {
                m.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
            }
            r.material = m;
        }

        SelectedEntity = null;
        EntityInfoRenderer.SelectedEntity = SelectedEntity;
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
            if (HoveredEntity.isAlive())
            {
                m.color = new Color(0, 0, 1, 1);
            }
            else
            {
                m.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
            }

            childRenderer.material = m;
        }

        HoveredEntity = null;
        EntityInfoRenderer.SelectedEntity = null;
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
