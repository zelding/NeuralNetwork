﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour
{
    public GameObject fishList;
    public GameObject foodList;

    public LayerMask EntityLayer;
    public LayerMask ObstacleLayer;

    [Header("Current Entities")]
    public EntityController SelectedEntity;
    public EntityController HoveredEntity;

    [Header("Entity info renderers")]
    private EntityInfoRenderer EntityInfoRenderer;
    private EntityInfoRenderer BestEntityInfoRenderer;
    private EntityInfoRenderer WorstEntityInfoRenderer;

    public Camera TopDownCamera;
    public Camera FollowingCamera;

    public Text txt_pop_size;
    public Text txt_generation;
    public Text txt_food;

    public float scrollSpeed;

    private Camera currentCamera;
    private Vector3 mainCameraPosition;
    private float mainCameraSize;

    [ Range(0, 100)]
    public int startingFishes = 50;
    public GameObject fishBody;
    [Range(0, 100)]
    public int startingfood = 50;
    public GameObject foodBody;

    private ThirdPersonCameraController thirdPersonCameraController;

    private float spawnBoundary = 430f;
    private bool isRunning      = true;
    public int Generation       = 0;

    private bool cycleEntities = false;
    private float lastCycle    = 0f;

    public List<EntityController> AliveEntities;
    public List<EntityController> DeadEntities;
    public List<FoodController> Food;

    public List<EntityState> WorstEntities;
    public List<EntityState> BestEntities;
    public List<EntityState> MiddleEntities;

    public void RegisterNewEntity(EntityController entity)
    {
        AliveEntities.Add(entity);
    }

    public void UnRegisterEntity( EntityController entity )
    {
        AliveEntities.Remove(entity);
        DeadEntities.Add(entity);
    }

    public void ReportFoodEaten( FoodController food )
    {
        Food.Remove(food);
        Destroy(food.gameObject);
    }

    private void Awake()
    {
        EntityInfoRenderer[] rr = FindObjectsOfType<EntityInfoRenderer>();

        if (rr.Length > 2) {
            BestEntityInfoRenderer = rr[ 0 ];
            EntityInfoRenderer = rr[ 1 ];
            WorstEntityInfoRenderer= rr[ 2 ];
        }
        else {
            BestEntityInfoRenderer = rr[ 0 ];
            EntityInfoRenderer = rr[ rr.Length - 1 ];
        }
    }

    // Use this for initialization
    void Start()
    {
        TopDownCamera.enabled = true;
        mainCameraPosition = TopDownCamera.transform.position;
        mainCameraSize = TopDownCamera.transform.position.y;

        FollowingCamera.enabled = false;
        currentCamera = TopDownCamera;

        Generation     = 1;
        AliveEntities  = new List<EntityController>();
        DeadEntities   = new List<EntityController>();
        Food           = new List<FoodController>();
        WorstEntities  = new List<EntityState>();
        BestEntities   = new List<EntityState>();
        MiddleEntities = new List<EntityState>();

        thirdPersonCameraController = GetComponent<ThirdPersonCameraController>();
        thirdPersonCameraController.enabled = false;

        CreateInitialPopulation();
    }

    // Update is called once per frame
    void Update()
    {
        DetectMouseEvents();
        DetectKeyboardEvents();
    }

    private void LateUpdate()
    {
        isRunning = AliveEntities.Count > 0;

        if( !isRunning )
        {
            if( DeadEntities.Count > 0 )
            {
                CreateChildrenEntities();

                BestEntityInfoRenderer.SelectedEntity  = BestEntities[ BestEntities.Count - 1 ];
                WorstEntityInfoRenderer.SelectedEntity = WorstEntities[ WorstEntities.Count - 1 ];
            }
        }
        else {
            if ( SelectedEntity == null && EntityInfoRenderer.SelectedEntity != null ) {
                EntityInfoRenderer.SelectedEntity = null;
            }

            FillMissingFood();
        }
    }

    private void OnGUI()
    {
        txt_pop_size.text = "Population: " + AliveEntities.Count +
                " / " + DeadEntities.Count +
                " / " + startingFishes;
        txt_generation.text = "Generation: " + Generation;
        txt_food.text = "Food: " + FindObjectsOfType<FoodController>().Length +
                " / " + Food.Count +
                " / " + startingfood;

        if( SelectedEntity != null && EntityInfoRenderer.SelectedEntity == null )
        {
            EntityInfoRenderer.SelectedEntity = SelectedEntity;
        }

        if( cycleEntities && lastCycle > 5f )
        {
            lastCycle = 0;
        }
        else
        {
            lastCycle += Time.smoothDeltaTime;
        }
    }

    private void CreateInitialPopulation()
    {
        for( int i = 0; i < startingFishes; i++ ) {
            var startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));

            while( IsCloseToOthers(startPosition) ) {
                startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));
            };

            GameObject fish = Instantiate(fishBody, startPosition, Quaternion.identity, fishList.transform);

            Vector3 euler = fish.transform.eulerAngles;
            euler.y = Random.Range(-180f, 180f);
            fish.transform.eulerAngles = euler;
        }

        FillMissingFood();

        isRunning = true;
    }

    private void CreateChildrenEntities()
    {
        Generation++;

        DeadEntities.Sort();

        int count = DeadEntities.Count;
        int middle = (count % 2 == 0) ? count / 2 : (count + 1) / 2;

        var nextGeneration = new List<EntityController>();

        for( int i = middle; i < count; i++ )
        {
            for( int k = 0; k < 2; k++ )
            {
                var startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));

                do
                {
                    startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));
                } while( IsCloseToOthers(startPosition) );

                GameObject       fish     = Instantiate(fishBody, startPosition, Quaternion.identity, fishList.transform);
                EntityController fishSoul = fish.GetComponent<EntityController>();

                Vector3 euler = fish.transform.eulerAngles;
                euler.y = Random.Range(-180f, 180f);
                fish.transform.eulerAngles = euler;

                fishSoul.InheritFrom(DeadEntities[ i ]);
                nextGeneration.Add(fishSoul);
            }
        }

        WorstEntities.Add(DeadEntities[ 0 ].GetSnapshot());
        BestEntities.Add(DeadEntities[ count - 1 ].GetSnapshot());
        MiddleEntities.Add(DeadEntities[ middle ].GetSnapshot());

        foreach( EntityController entity in DeadEntities )
        {
            Destroy(entity.gameObject);
        }

        DeadEntities.Clear();
        AliveEntities.Clear();
        AliveEntities = nextGeneration;

        isRunning = true;
    }

    private void FillMissingFood()
    {
        int food = FindObjectsOfType<FoodController>().Length;

        if( food < startingfood ) {
            for( int i = 0; i < startingfood - food; i++ ) {
                var startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), 1.5f, Random.Range(-spawnBoundary, spawnBoundary));

                Instantiate(foodBody, startPosition, Quaternion.identity, foodList.transform);

                Food.Add(foodBody.GetComponent<FoodController>());
            }
        }
    }

    private void DetectMouseEvents()
    {
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if( Physics.Raycast(ray, out hit, EntityLayer) )
        {
            GameObject HoveredGameObject = hit.transform.gameObject;

            EntityController foundFish = HoveredGameObject.GetComponent<EntityController>();

            if( foundFish != null )
            {
                if( Input.GetMouseButtonDown(0) )
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

        if ( TopDownCamera.enabled )
        {
            if ( Input.mouseScrollDelta.magnitude > 0 )
            {
                mainCameraSize = Input.mouseScrollDelta.y * scrollSpeed;

                if( TopDownCamera.enabled ) {
                    //TopDownCamera.orthographicSize   = mainCameraSize;
                    TopDownCamera.transform.position += Vector3.up * mainCameraSize;
                    //TopDownCamera.transform.position = mainCameraPosition;
                }
            }
        }
    }

    private void DetectKeyboardEvents()
    {
        if( Input.GetKeyDown(KeyCode.F1) || (Input.GetMouseButtonDown(1) && FollowingCamera.enabled) )
        {
            ActivateTopDownCamera();
        }

        if( Input.GetKeyDown(KeyCode.F2) || (Input.GetMouseButtonDown(1) && TopDownCamera.enabled) )
        {
            ActivateFollowerCamera();
        }

        if( Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(2) )
        {
            ActivateTopDownCamera();
            ClearSelection();
        }

        if( TopDownCamera.enabled ) {
            if( Input.GetAxisRaw("Horizontal") != 0 ) {
                mainCameraPosition += Vector3.right * Input.GetAxisRaw("Horizontal") * scrollSpeed;
            }

            if( Input.GetAxisRaw("Vertical") != 0 ) {
                mainCameraPosition += Vector3.forward * Input.GetAxisRaw("Vertical") * scrollSpeed;
            }

        }
    }

    private void SelectFish( EntityController obj )
    {
        if( obj == null || obj == SelectedEntity )
        {
            return;
        }

        if( SelectedEntity != null )
        {
            ClearSelection(true);
        }

        SelectedEntity = obj;
        thirdPersonCameraController.Target = SelectedEntity.transform;
        thirdPersonCameraController.turnSpeed = SelectedEntity.Genes.Legs.turnSpeed;
        thirdPersonCameraController.smoothSpeed = SelectedEntity.Genes.Legs.smoothSpeed;

        Renderer childRenderers = SelectedEntity.GetComponent<Renderer>();
        
        Material m = childRenderers.material;

        if( SelectedEntity.isAlive() )
        {
            m.color = EntityController.SelectedAliveColor;
            SelectedEntity.EnableEyeLashes();
        }
        else
        {
            m.color = EntityController.SelectedDeadColor;
        }

        childRenderers.material = m;

        ClearHover();

        EntityInfoRenderer.SelectedEntity = SelectedEntity;
        SelectedEntity.AllowRender = true;
    }

    private void HoverSelectable( EntityController obj )
    {
        if( obj == HoveredEntity || obj == SelectedEntity )
        {
            return;
        }

        if( obj == null )
        {
            ClearHover();
        }
        else
        {
            Renderer r = obj.GetComponent<Renderer>();
            Material m = r.material;

            if( obj.isAlive() )
            {
                m.color = EntityController.HoverAliveColor;
            }
            else
            {
                m.color = EntityController.HoverDeadColor;
            }

            m.color = Color.green;
            r.material = m;

            HoveredEntity = obj;
        }

        EntityInfoRenderer.SelectedEntity = HoveredEntity;
    }

    /// <summary>
    /// 
    /// </summary>
    private void ClearSelection( bool onlyResetColors = false )
    {
        if( SelectedEntity == null )
        {
            return;
        }

        if( !onlyResetColors )
        {
            thirdPersonCameraController.enabled = false;
            thirdPersonCameraController.Target = null;
        }

        Renderer r = SelectedEntity.GetComponent<Renderer>();
 
        Material m = r.material;
        if( SelectedEntity.isAlive() )
        {
            m.color = EntityController.AliveColor;
        }
        else
        {
            m.color = EntityController.DeadColor;
        }
        r.material = m;

        SelectedEntity.AllowRender = false;
        SelectedEntity.DisableEyeLashes();
        SelectedEntity = null;
        EntityInfoRenderer.SelectedEntity = SelectedEntity;
    }

    private void ClearHover()
    {
        if( HoveredEntity == null || SelectedEntity == HoveredEntity )
        {
            return;
        }

        Renderer r = HoveredEntity.GetComponent<Renderer>();

        Material m = r.material;
        if( HoveredEntity.isAlive() )
        {
            m.color = EntityController.AliveColor;
        }
        else
        {
            m.color = EntityController.DeadColor;
        }

        r.material = m;

        HoveredEntity = null;
        EntityInfoRenderer.SelectedEntity = SelectedEntity;
    }

    private bool IsCloseToOthers( Vector3 pos )
    {
        if( AliveEntities.Count == 0 ) return false;

        foreach( EntityController entity in AliveEntities )
        {
            float min_x = entity.transform.position.x;
            float max_x = entity.transform.position.x + entity.transform.localScale.x;
            float min_z = entity.transform.position.z;
            float max_z = entity.transform.position.z + entity.transform.localScale.z;

            if( pos.x > min_x && pos.x < max_x && pos.z > min_z && pos.z < max_z )
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

        cycleEntities = false;
    }

    private void ActivateFollowerCamera()
    {
        TopDownCamera.enabled = false;
        FollowingCamera.enabled = true;
        thirdPersonCameraController.enabled = true;
        currentCamera = FollowingCamera;

        cycleEntities = true;
    }
}
