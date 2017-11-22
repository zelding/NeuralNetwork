using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour, System.IComparable<EntityController>, EntityInfo
{
    public const int NumberOfInputs = 15;
    public const int NumberOfOutputs = 4;
    public const int NumberOfMemoryNeurons = 0;

    public static Color AliveColor;
    public static Color DeadColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
    public static Color SelectedAliveColor = new Color(1, 0, 0, 1);
    public static Color SelectedDeadColor = new Color(0.67f, 0, 0, 0.87f);
    public static Color HoverAliveColor = new Color(0, 1, 0, 1);
    public static Color HoverDeadColor = new Color(0, 0.67f, 0, 0.87f);

    public static Color MovingHeadColor = new Color(1, 1, 0, 1);
    public static Color StillHeadColor = new Color(0, 0, 1, 1);

    public string Name { get; private set; }
    public string BaseName;
	//public VectorNet Brain;
	public NeuralNetwork Brain;
    public Genes Genes;
    public EightDirController Legs;
    public FieldOfView Eye;
    public Nostrils Nose;
    public Sonar Ears;
    public Transform Body;
    public Rigidbody Bones;

    public int generation;
    public int variant;
    public int revision;

    public bool AllowRender;

    [Header("Hmmmmmm")]
    public bool Immortal = false;
    public bool CanMove = true;

    [Range(500f, 5000f)]
    public float MaxEnergy;

    [Range(10, 100)]
    public float FeedingTimer;

    private float Energy;
    private float CurrrentFeedingTimer;

    //public Vector3[] Output { get; private set; }
    //public Vector3[] Input { get; set; }

	public float[] Output { get; private set; }
	public float[] Input { get; set; }

    public string NeuStr;

    public float Age { get; private set; }
    public float Distance { get; private set; }
    public float Consumption { get; private set; }

    private bool markedAsDead = false;
    private bool eaten = false;

    private bool isInited = false;

    private Vector3 lastPosition;
    private float displacement;
    private float speed;
    private float topSpeed;

    public Transform lastNoseTartget;
    public Transform lastEyeTarget;

    private SimulationManager GOD;

    public void Awake()
    {
        Eye = GetComponentInChildren<FieldOfView>();
        Body = GetComponent<Transform>();
        Nose = GetComponentInChildren<Nostrils>();
        Ears = GetComponentInChildren<Sonar>();
        Bones = GetComponent<Rigidbody>();
        GOD = FindObjectOfType<SimulationManager>();
    }

    public void InheritFrom(EntityController entity)
    {
		//Brain = new VectorNet(entity.Brain);
		Brain = new NeuralNetwork(entity.Brain);
        Genes = new Genes(entity.Genes);

        generation = (int)Brain.gen;
        variant = entity.variant;
        revision = entity.revision;

        if (Brain.isMutated) {
            revision++;
        }

        if (Genes.isMutated) {
            variant++;
        }
        BaseName = entity.BaseName;
        Name = BaseName + " " + generation + "." + variant + "." + revision;

        Immortal = entity.Immortal;

        isInited = true;
    }

    public EntityState GetSnapshot()
    {
        return new EntityState(this);
    }

    public float GetSpeed()
    {
        return speed;
    }

    public float GetHunger()
    {
        return CurrrentFeedingTimer;
    }

    public void Kill()
    {
        Energy = 0;
    }

    // Use this for initialization
    void Start()
    {
        if (!isInited) {
            Genes = new Genes();

            List<int> brainStructure = new List<int>();

            brainStructure.Add(NumberOfInputs + NumberOfMemoryNeurons);

            foreach (int layerLength in Genes.Brain.neuronsInHiddenLayers) {
                brainStructure.Add(layerLength);
            }

            brainStructure.Add(NumberOfOutputs + NumberOfMemoryNeurons);

			//Brain = new VectorNet(brainStructure.ToArray());
			Brain = new NeuralNetwork(brainStructure.ToArray());

            BaseName = Collections.Names[Random.Range(0, Collections.Names.Count)];
            Name = BaseName + " 1.0.0";
        }

        Legs = new EightDirController(this);

        Distance = 0f;

        AliveColor = Genes.Color.GetColor();
        ResetColor();

        Output = new float[NumberOfOutputs];

        name = "Fish " + Name;

        Age = 0;
		Consumption = 0f;

        Energy = MaxEnergy;
        CurrrentFeedingTimer = FeedingTimer;

        displacement = 0;
        speed = 0;
        topSpeed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled && (Immortal || Energy > 0)) // marked as dead doesn't matter around here
        {
            Vector3 noseInput = Vector3.zero;
            Vector3 eyeInput = Vector3.zero;

            if (Nose.visibleTargets.Count > 0) {
                float minSqrDst = float.MaxValue;
                lastNoseTartget = null;

                foreach (Transform t in Nose.visibleTargets) {
					if (t != null) {
						float sqrDst = (transform.position - t.position).sqrMagnitude;

						if (sqrDst < minSqrDst) {
							lastNoseTartget = t;
							minSqrDst = sqrDst;
						}
					}
                }
            }

            if (Eye.visibleTargets.Count > 0) {
                float minSqrDst = float.MaxValue;
                lastEyeTarget = null;

                foreach (Transform t in Eye.visibleTargets) {
					if (t != null) {
						float sqrDst = (transform.position - t.position).sqrMagnitude;

						if (sqrDst < minSqrDst) {
							lastEyeTarget = t;
							minSqrDst = sqrDst;
						}
					}
                }
            }

            if (lastEyeTarget != null) {
                eyeInput = (lastEyeTarget.transform.position - Body.transform.position).normalized;
                //eyeInput = transform.InverseTransformDirection(eyeInput);
            }

            if (lastNoseTartget != null) {
                noseInput = (lastNoseTartget.transform.position - Body.transform.position).normalized;
                //noseInput = transform.InverseTransformDirection(noseInput);
            }

			float[] EarsData = Ears.GetData();

            /*Input = new Vector3[NumberOfInputs + NumberOfMemoryNeurons] {
                eyeInput,
                noseInput,
                EarsData,
                new Vector3(((Energy / MaxEnergy) - 0.5f) * 2, 0, 0),
                Output[ Output.Length - 1]
            };*/
			Input = new float[NumberOfInputs + NumberOfMemoryNeurons] {
				eyeInput.x,
				eyeInput.y,
				eyeInput.z,
				noseInput.x,
				noseInput.y,
				noseInput.z,
				EarsData[0],
				EarsData[1],
				EarsData[2],
				EarsData[3],
				EarsData[4],
				((Energy / MaxEnergy) - 0.5f) * 2,
				Output[0],
				Output[1],
				Output[2]
			};

            Output = Brain.FeedForward(Input);

            Age += Time.deltaTime;

            Vector3 baseOffset = transform.position;
            float age = Mathf.Clamp(Age / 25, 1, 25);
            float step = Mathf.SmoothStep(age, age * 1.1f, Time.fixedDeltaTime);

            transform.localScale = new Vector3(transform.localScale.x, step, transform.localScale.z);

            Distance += Vector3.Distance(lastPosition, transform.position);
            displacement += Vector3.Distance(lastPosition, transform.position);

            lastPosition = transform.position;

            if (CurrrentFeedingTimer <= 0) {
                UseEnergy(Time.deltaTime * 5);
            }
            else {
                CurrrentFeedingTimer -= Time.deltaTime;
            }

            //Legs.HandleInput(Output[0].x, Output[0].z, Output[0].magnitude);
			Vector3 movement = new Vector3(Output[0], Output[1], Output[2]);
			//Vector3 movement = new Vector3(noseInput.x, noseInput.y, noseInput.z);

			//float dst = Vector3.Distance (noseInput, transform.position);

			Legs.Handle3DMovement(movement);
        }
        else {
            if (!markedAsDead) {
                Renderer r = GetComponent<Renderer>();

                Material m = r.material;
                m.color = DeadColor;
                r.material = m;

                markedAsDead = true;
                enabled = false;

				if (Eye.enabled) {
					Eye.enabled = false;
				}

				if (Nose.enabled) {
					Nose.enabled = false;
				}
            }
        }

        if (lastNoseTartget != null && Nose.visibleTargets.Count == 0) {
            lastNoseTartget = null;
        }

        if (lastEyeTarget != null && Eye.visibleTargets.Count == 0) {
            lastEyeTarget = null;
        }
    }

    void OnEnable()
    {
        //register myself at GOD
        GOD.RegisterNewEntity(this);
    }

    void OnDisable()
    {
        //unregister at GOD
		AllowRender = false;
        DisableEyeLashes();
        GOD.UnRegisterEntity(this);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (Energy > 0) {
            if (collision.gameObject.tag == "Fish") {
                EntityController otherFish = collision.gameObject.GetComponentInParent<EntityController>();

                if (otherFish != null && !otherFish.isAlive() && !otherFish.eaten) {
                    otherFish.eaten = true;
                    GainEnergy(1000);
                    Consumption += 2;
                    otherFish.transform.position += new Vector3(0, -10000, 0);

                    CurrrentFeedingTimer += FeedingTimer * 2;
                }
            }

            if (collision.gameObject.tag == "Food") {
                FoodController food = collision.gameObject.GetComponentInParent<FoodController>();

                if (food != null) {
                    GainEnergy(500);
                    Consumption += 1;
                    GOD.ReportFoodEaten(food);
                    CurrrentFeedingTimer += FeedingTimer;
                }
            }

            if (collision.gameObject.tag == "Wall") {
                Consumption /= 2f;
                UseEnergy(Energy);
            }
        }
    }

	void OnTriggerExit(Collider other)
	{
		if (other.tag == "Wall") {
			Consumption /= 2f;
			UseEnergy(Energy);
		}
	}

    void FixedUpdate()
    {
        if (Energy > 0) {
            if (displacement > 0) {
                speed = Mathf.Clamp((float)System.Math.Round(displacement / Time.fixedDeltaTime, 2), 0, 100);
                displacement = 0;
            }

            if (100 > speed && speed > topSpeed) {
                topSpeed = speed;
            }

            if( CanMove ) {
                //Legs.Move();
                Legs.Move3D();
            }
        }
    }

    void OnDrawGizmos()
    {
        if (Energy > 0 && AllowRender) {
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, transform.forward * (speed + transform.localScale.z));

            if (lastNoseTartget != null) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(lastNoseTartget.position, 4f);
                Gizmos.DrawWireSphere(transform.position, Genes.Noze.range);
            }

            if (Nose.enabled && Nose.visibleTargets.Count > 0) {
                foreach (Transform vt in Nose.visibleTargets) {
                    if (vt != null) {
                        Gizmos.DrawLine(vt.position, transform.position);
                    }
                }
            }

            if (lastEyeTarget != null) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(lastEyeTarget.position, 13f);
                Gizmos.DrawWireSphere(transform.position, Genes.Eyes.range);
            }

            if (Eye.enabled && Eye.visibleTargets.Count > 0) {
                foreach (Transform vt in Eye.visibleTargets) {
                    if (vt != null) {
                        Gizmos.DrawLine(vt.position, transform.position);
                    }
                }
            }

			float[] EarsData = Ears.GetData();

			Vector3 forward     = transform.position + (transform.forward * Genes.Ears.range);
			//Vector3 rightVector = Quaternion.AngleAxis(45, transform.up) * transform.forward;
			//Vector3 leftVector  = Quaternion.AngleAxis(-45, transform.up) * transform.forward;

			Vector3 rightUpVector   = Quaternion.AngleAxis(45, transform.up) * Quaternion.AngleAxis(45, transform.right) * transform.forward;
			Vector3 rightDownVector = Quaternion.AngleAxis(45, transform.up) * Quaternion.AngleAxis(-45, transform.right) * transform.forward;

			Vector3 leftUpVector    = Quaternion.AngleAxis(-45, transform.up) * Quaternion.AngleAxis(45, transform.right)  * transform.forward;
			Vector3 leftDownVector  = Quaternion.AngleAxis(-45, transform.up) * Quaternion.AngleAxis(-45, transform.right) * transform.forward;

            if (EarsData[0] > 0) {
                Gizmos.color = Color.cyan;
				Gizmos.DrawLine(transform.position, forward);
            }
            else {
                Gizmos.color = new Color(0, 0.34f, 0.34f, 1);
				Gizmos.DrawLine(transform.position, forward);
            }

            if (EarsData[1] > 0) {
                Gizmos.color = Color.cyan;
				Gizmos.DrawLine(transform.position, transform.position + leftUpVector * Genes.Ears.range);
            }
            else {
                Gizmos.color = new Color(0, 0.34f, 0.34f, 1);
				Gizmos.DrawLine(transform.position, transform.position + leftUpVector * Genes.Ears.range);
            }

			if (EarsData[2] > 0) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(transform.position, transform.position + leftDownVector * Genes.Ears.range);
			}
			else {
				Gizmos.color = new Color(0, 0.34f, 0.34f, 1);
				Gizmos.DrawLine(transform.position, transform.position + leftDownVector * Genes.Ears.range);
			}

			if (EarsData[3] > 0) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(transform.position, transform.position + rightUpVector * Genes.Ears.range);
			}
			else {
				Gizmos.color = new Color(0, 0.34f, 0.34f, 1);
				Gizmos.DrawLine(transform.position, transform.position + rightUpVector * Genes.Ears.range);
			}

			if (EarsData[4] > 0) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(transform.position, transform.position + rightDownVector * Genes.Ears.range);
			}
			else {
				Gizmos.color = new Color(0, 0.34f, 0.34f, 1);
				Gizmos.DrawLine(transform.position, transform.position + rightDownVector * Genes.Ears.range);
			}
        }
    }

    #region utility

    public void UseEnergy(float amount)
    {
        if (Immortal) {
            return;
        }

        Energy -= Mathf.Abs(amount);
    }

    public void GainEnergy(float amount)
    {
        if (Immortal) {
            return;
        }

        Energy += Mathf.Abs(amount);
    }

    public void EnableEyeLashes()
    {
        Eye.allowRender = true;
    }

    public void DisableEyeLashes()
    {
        Eye.allowRender = false;
    }

    public void ResetColor()
    {
        Renderer r = GetComponent<Renderer>();

        Material m = r.material;

        if (isAlive()) {
            m.color = Genes.Color.GetColor();
        }
        else {
            m.color = DeadColor;
        }
        r.material = m;
    }

    #endregion

    #region ICopmare

    public int CompareTo(EntityController other)
    {
        if (other == null) return 1;

        return GetFittness().CompareTo(other.GetFittness());
    }

    #endregion

    #region EntityInfo

    public string GetName()
    {
        return Name;
    }

    public string GetNeuronString()
    {
        return NeuStr;
    }

    public NeuralNetwork GetBrain()
    {
        return Brain;
    }

    public Genes GetGenes()
    {
        return Genes;
    }

    public float GetEnergy()
    {
        return Energy;
    }

    public float GetAge()
    {
        return Age;
    }

    public float GetDistance()
    {
        return Distance;
    }

    public float GetConsumption()
    {
        return Consumption;
    }

    public bool isAlive()
    {
        return Immortal || (!markedAsDead || Energy > 0);
    }

    public float GetFittness()
    {
		return GetAge() * GetConsumption() + GetAge();
    }

    public float GetTopSpeed()
    {
        return topSpeed;
    }

    #endregion
}
