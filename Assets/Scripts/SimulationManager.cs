using System.Collections.Generic;
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
    public Text txt_gens;
    public Text txt_alives;
    public Text txt_names;
	public Text txt_curr_names;
	public Text txt_global_time;
	public Text txt_cycle_time;

    public float scrollSpeed;

    private Camera currentCamera;
    private Vector3 mainCameraPosition;
    private float mainCameraSize;

    [Range(0, 500)]
    public int startingFishes = 50;
    public GameObject fishBody;
    [Range(0, 1000)]
    public int startingFood = 50;
    public GameObject foodBody;

    private ThirdPersonCameraController thirdPersonCameraController;

    private float spawnBoundary = 1000f;
    private bool isRunning = true;
	private bool startNewCycleAsap = false;

	private float globalTime;
	private float cycleTime;

    public int Cycle = 0;
    public uint minGen = uint.MaxValue;
    public uint maxGen = 0;

    public SortedDictionary<int, int> EntityCount;
    public SortedDictionary<string, int> EntityNames;
    public SortedDictionary<string, int> CurrentEntityNames;

    public List<EntityController> AliveEntities;
    public List<EntityController> DeadEntities;
    public List<FoodController> Food;
    public SortedDictionary<string, FoodController> FoodList;

    public List<EntityState> WorstEntities;
    public List<EntityState> BestEntities;
    public List<EntityState> MiddleEntities;

    public void RegisterNewEntity( EntityController entity )
    {
        if( isRunning ) {
            AliveEntities.Add(entity);
        }
    }

    public void RegisterNewFood( FoodController food )
    {
        Food.Add(food);
        FoodList.Add(food.ID, food);
    }

    public void UnRegisterEntity( EntityController entity )
    {
        if( isRunning ) {
            int gen = (int) entity.Brain.gen;

			if( !EntityCount.ContainsKey((int) gen) ) {
				EntityCount[ (int) gen ] = 1;
			}

			EntityNames [entity.BaseName]++;

			EntityController fish = CreateNewFish (entity);

			AliveEntities.Remove (entity);
			EntityCount [gen]--;
			DeadEntities.Add (entity);

			if (gen <= Cycle && (!EntityCount.ContainsKey (gen) || EntityCount [gen] == 0)) {
				startNewCycleAsap = true;
			}
        }
    }

	EntityController CreateNewFish(EntityController parent = null) 
	{
		var startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), Random.Range(-spawnBoundary, spawnBoundary), Random.Range(-spawnBoundary, spawnBoundary));

		do {
			startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), Random.Range(-spawnBoundary, spawnBoundary), Random.Range(-spawnBoundary, spawnBoundary));
		} while( IsCloseToOthers(startPosition) );

		GameObject fish = Instantiate(fishBody, startPosition, Quaternion.identity, fishList.transform);
		EntityController fishSoul = fish.GetComponent<EntityController>();

		Vector3 euler = fish.transform.eulerAngles;
		euler.y = Random.Range(-180f, 180f);
		fish.transform.eulerAngles = euler;

		if (parent != null) {
			fishSoul.InheritFrom (parent);
		}

		return fishSoul;
	}

    EntityController CreateNewFish( Vector3 pos, EntityController parent = null )
    {
        GameObject fish = Instantiate(fishBody, pos, Quaternion.identity, fishList.transform);
        EntityController fishSoul = fish.GetComponent<EntityController>();

        Vector3 euler = fish.transform.eulerAngles;
        euler.y = Random.Range(-180f, 180f);
        fish.transform.eulerAngles = euler;

        if( parent != null ) {
            fishSoul.InheritFrom(parent);
        }

        return fishSoul;
    }


    public void ReportFoodEaten( FoodController food )
    {
        FoodList.Remove(food.ID);
        Food.Remove(food);
        Destroy(food.gameObject);

        FillMissingFood();
    }

    private void Awake()
    {
        EntityInfoRenderer[] rr = FindObjectsOfType<EntityInfoRenderer>();

        if( rr.Length > 2 ) {
            BestEntityInfoRenderer  = rr[ 0 ];
            EntityInfoRenderer      = rr[ 1 ];
            WorstEntityInfoRenderer = rr[ 2 ];
        }
        else if ( rr.Length == 2) {
            BestEntityInfoRenderer = rr[ 0 ];
            EntityInfoRenderer     = rr[ rr.Length - 1 ];
        }
        else {
            EntityInfoRenderer = rr[ 0 ];
        }

        EntityNames        = new SortedDictionary<string, int>();
        CurrentEntityNames = new SortedDictionary<string, int>();
    }

    // Use this for initialization
    void Start()
    {
		QualitySettings.vSyncCount = 0;
        TopDownCamera.enabled = true;
        mainCameraPosition = TopDownCamera.transform.position;
        mainCameraSize = TopDownCamera.transform.position.y;

        FollowingCamera.enabled = false;
        currentCamera = TopDownCamera;

        Cycle = 1;
        AliveEntities = new List<EntityController>();
        DeadEntities = new List<EntityController>();
        Food = new List<FoodController>();
        WorstEntities = new List<EntityState>();
        BestEntities = new List<EntityState>();
        MiddleEntities = new List<EntityState>();

        FoodList = new SortedDictionary<string, FoodController>();

        thirdPersonCameraController = GetComponent<ThirdPersonCameraController>();
        thirdPersonCameraController.enabled = false;

        CreateInitialPopulation();
        FillMissingFood();

        //EntityController fish = CreateNewFish(Vector3.zero);
        //fish.CanMove = false;

        EntityCount = new SortedDictionary<int, int>();

        //Time.timeScale = 2;
    }

    // Update is called once per frame
    void Update()
    {
        DetectMouseEvents();
        DetectKeyboardEvents();

        UpdateNameList();

		cycleTime += Time.unscaledDeltaTime;
    }

    private void LateUpdate()
    {
        if( isRunning ) {
            if( SelectedEntity == null && EntityInfoRenderer.SelectedEntity != null ) {
                EntityInfoRenderer.SelectedEntity = null;
            }

            minGen = uint.MaxValue;
            maxGen = 0;
            EntityCount = new SortedDictionary<int, int>();

            foreach( EntityController e in AliveEntities ) {
                var asd = e.Brain.gen;

                if( asd > maxGen ) {
                    maxGen = asd;
                }
                else if( asd < minGen ) {
                    minGen = asd;
                }

                if( !EntityCount.ContainsKey((int) asd) ) {
                    EntityCount[ (int) asd ] = 0;
                }

                EntityCount[ (int) asd ]++;
            }
        }

		if ( startNewCycleAsap ) {
			isRunning = false;

			if( startingFishes > 0 && DeadEntities.Count >= startingFishes ) {
				CreateChildrenEntities();

				BestEntityInfoRenderer.SelectedEntity = BestEntities[ BestEntities.Count - 1 ];
				WorstEntityInfoRenderer.SelectedEntity = WorstEntities[ WorstEntities.Count - 1 ];

				startNewCycleAsap = false;
			}

			isRunning = true;
		}
    }

    private void OnGUI()
    {
		if (isRunning) {
			txt_pop_size.text = (isRunning ? "Y" : "N") + " Population: " + AliveEntities.Count +
			" / " + DeadEntities.Count +
			" / " + startingFishes;
			txt_generation.text = "Cycle: " + Cycle + ", Generations: " + minGen + " - " + maxGen;
			txt_food.text = "Food: " + FoodList.Count + " / " + startingFood;

			int minutes = Mathf.FloorToInt(Time.unscaledTime / 60f);
			int seconds = Mathf.FloorToInt(Time.unscaledTime - minutes * 60);
			string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);

			txt_global_time.text = "All time: " + niceTime;

			minutes = Mathf.FloorToInt(cycleTime / 60f);
			seconds = Mathf.FloorToInt(cycleTime - minutes * 60);
			niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);

			txt_cycle_time.text = "Cycle time: " + niceTime;

			string g = "";
			string a = "";
			string n = "";
			string c = "";

			foreach (int k in EntityCount.Keys) {
				g += k + "\n";
			}

			foreach (int v in EntityCount.Values) {
				a += v + "\n";
			}

			foreach (string v in EntityNames.Keys) {
				n += v + ": " + EntityNames [v] + "\n";
			}

			foreach (string v in CurrentEntityNames.Keys) {
				c += v + ": " + CurrentEntityNames [v] + "\n";
			}

			txt_gens.text = g;
			txt_alives.text = a;
			txt_names.text = n;
			txt_curr_names.text = c;
		}
    }

    private void UpdateNameList()
    {
        if ( EntityNames == null ) {
            EntityNames = new SortedDictionary<string, int>();
        }

        foreach( string s in CurrentEntityNames.Keys ){
            CurrentEntityNames[ s ] = 0;
        }

        if ( AliveEntities.Count > 0 ) {
            foreach (EntityController entity in AliveEntities) {
                if( !EntityNames.ContainsKey(entity.BaseName) ) {
                    EntityNames.Add(entity.BaseName, 1);
                }

                if( !CurrentEntityNames.ContainsKey(entity.BaseName) ) {
                    CurrentEntityNames.Add(entity.BaseName, 0);
                }

                CurrentEntityNames[ entity.BaseName ]++;
            }
        }
    }

    private void CreateInitialPopulation()
    {
        for( int i = 0; i < startingFishes; i++ ) {
			CreateNewFish ();
        }

        isRunning = true;
    }

    private void CreateChildrenEntities()
    {
		Cycle = (int)minGen;

		foreach (EntityController e in AliveEntities) {
			Destroy (e.gameObject);
		}

        AliveEntities.Clear();
        DeadEntities.Sort();

        int deadCount = DeadEntities.Count;
        int count = Mathf.RoundToInt(deadCount - (startingFishes / 2f));
        int middle = (deadCount % 2 == 0) ? count / 2 : (count + 1) / 2;

        var nextGeneration = new List<EntityController>();

		int x = 0;
        for( int i = count; i < deadCount; i++ ) {
            for( int k = 0; k < 2; k++ ) {
				EntityController fish = CreateNewFish (DeadEntities[ i ]);

				nextGeneration.Add(fish);
				x++;
            }
        }

        WorstEntities.Add(DeadEntities[ 0 ].GetSnapshot());
        BestEntities.Add(DeadEntities[ DeadEntities.Count - 1 ].GetSnapshot());
        MiddleEntities.Add(DeadEntities[ middle ].GetSnapshot());

        foreach( EntityController entity in DeadEntities ) {
            Destroy(entity.gameObject);
        }

        DeadEntities.Clear();
        AliveEntities = nextGeneration;

        minGen = uint.MaxValue;
        maxGen = 0;
		cycleTime = 0;
    }

    private void FillMissingFood()
    {
        int food = FoodList.Count;

        if( food < startingFood ) {
            for( int i = 0; i < startingFood - food; i++ ) {
				var startPosition = new Vector3(Random.Range(-spawnBoundary, spawnBoundary), Random.Range(-spawnBoundary, spawnBoundary), Random.Range(-spawnBoundary, spawnBoundary));

                Instantiate(foodBody, startPosition, Quaternion.identity, foodList.transform);
            }
        }
    }

    private void DetectMouseEvents()
    {
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

		if( Physics.Raycast(ray, out hit, EntityLayer) ) {
            GameObject HoveredGameObject = hit.transform.gameObject;

            EntityController foundFish = HoveredGameObject.GetComponent<EntityController>();

            if( foundFish != null ) {
                if( Input.GetMouseButtonDown(0) ) {
                    SelectFish(foundFish);
                }
                else {
                    HoverSelectable(foundFish);
                }
            }
            else {
                ClearHover();
            }
        }
        else {
            ClearHover();
        }
    }

    private void DetectKeyboardEvents()
    {
        if( Input.GetKeyDown(KeyCode.F1) || (Input.GetMouseButtonDown(1) && FollowingCamera.enabled) ) {
            ActivateTopDownCamera();
        }

        if( Input.GetKeyDown(KeyCode.F2) || (Input.GetMouseButtonDown(1) && TopDownCamera.enabled) ) {
            ActivateFollowerCamera();
        }

        if( Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(2) ) {
            ActivateTopDownCamera();
            ClearSelection();
        }

        /*if( TopDownCamera.enabled ) {
            if( Input.GetAxisRaw("Horizontal") != 0 ) {
                mainCameraPosition -= Vector3.right * Input.GetAxisRaw("Horizontal") * scrollSpeed;
            }

            if( Input.GetAxisRaw("Vertical") != 0 ) {
                mainCameraPosition -= Vector3.forward * Input.GetAxisRaw("Vertical") * scrollSpeed;
            }

			if( Input.mouseScrollDelta.y != 0 ) {
				mainCameraSize = Input.mouseScrollDelta.y * scrollSpeed;
				mainCameraPosition -= Vector3.up * mainCameraSize;
			}

            TopDownCamera.transform.position = mainCameraPosition;
        }*/
    }

    private void SelectFish( EntityController obj )
    {
        if( obj == null || obj == SelectedEntity ) {
            return;
        }

        if( SelectedEntity != null ) {
            ClearSelection(true);
        }

        SelectedEntity = obj;
        thirdPersonCameraController.Target = SelectedEntity.transform;
        thirdPersonCameraController.turnSpeed = SelectedEntity.Genes.Legs.turnSpeed;
        thirdPersonCameraController.smoothSpeed = SelectedEntity.Genes.Legs.smoothSpeed;

        Renderer childRenderers = SelectedEntity.GetComponent<Renderer>();

        Material m = childRenderers.material;

        if( SelectedEntity.isAlive() ) {
            m.color = EntityController.SelectedAliveColor;
            SelectedEntity.EnableEyeLashes();
        }
        else {
            m.color = EntityController.SelectedDeadColor;
        }

        childRenderers.material = m;

        ClearHover();

        EntityInfoRenderer.SelectedEntity = SelectedEntity;
        SelectedEntity.AllowRender = true;
    }

    private void HoverSelectable( EntityController obj )
    {
        if( obj == HoveredEntity || obj == SelectedEntity ) {
            return;
        }

        if( obj == null ) {
            ClearHover();
        }
        else {
            Renderer r = obj.GetComponent<Renderer>();
            Material m = r.material;

            if( obj.isAlive() ) {
                m.color = EntityController.HoverAliveColor;
            }
            else {
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
        if( SelectedEntity == null ) {
            return;
        }

        if( !onlyResetColors ) {
            thirdPersonCameraController.enabled = false;
            thirdPersonCameraController.Target = null;
        }

        SelectedEntity.ResetColor();

        SelectedEntity.AllowRender = false;
        SelectedEntity.DisableEyeLashes();
        SelectedEntity = null;
        EntityInfoRenderer.SelectedEntity = SelectedEntity;
    }

    private void ClearHover()
    {
        if( HoveredEntity == null || SelectedEntity == HoveredEntity ) {
            return;
        }

        HoveredEntity.ResetColor();

        HoveredEntity = null;
        EntityInfoRenderer.SelectedEntity = SelectedEntity;
    }

    private bool IsCloseToOthers( Vector3 pos )
    {
        if( AliveEntities.Count == 0 ) return false;

        foreach( EntityController entity in AliveEntities ) {
            float min_x = entity.transform.position.x;
            float max_x = entity.transform.position.x + entity.transform.localScale.x;
            float min_z = entity.transform.position.z;
            float max_z = entity.transform.position.z + entity.transform.localScale.z;

            if( pos.x > min_x && pos.x < max_x && pos.z > min_z && pos.z < max_z ) {
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
