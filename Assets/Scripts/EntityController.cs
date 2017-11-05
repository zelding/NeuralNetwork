﻿﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Math;

public class EntityController : MonoBehaviour, System.IComparable<EntityController>, EntityInfo
{
    public static Color AliveColor = new Color(0, 0,1, 1);
    public static Color DeadColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
    public static Color SelectedAliveColor = new Color(1, 0, 0, 1);
    public static Color SelectedDeadColor = new Color(0.67f, 0, 0, 0.87f);
    public static Color HoverAliveColor = new Color(0, 1, 0, 1);
    public static Color HoverDeadColor = new Color(0, 0.67f, 0, 0.87f);

    public static Color MovingHeadColor = new Color(1, 1, 0, 1);
    public static Color StillHeadColor = new Color(0, 0, 1, 1);

    public string Name { get; private set; }
    public string BaseName;
    public NeuralNetwork Brain;
    public Genes Genes;
    public EightDirController Legs;
    public FieldOfView Eye;
    public Nostrils Nose;
    public Transform Body;
    public Rigidbody Bones;

    public int generation;
    public int variant;
    public int revision;

    public bool AllowRender;

    [Header("Hmmmmmm")]
    public bool Immortal = false;

    [Range(500f, 5000f)]
    public float MaxEnergy;

    [Range(10, 100)]
    public float FeedingTimer;

    private float Energy;
    private float CurrrentFeedingTimer;

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
    private Vector3 guiLastPosition;
    private float displacement;
    private float speed;
    private float topSpeed;

    public Transform lastNoseTartget;
    public Transform lastEyeTarget;

    private int noseTargetIndex;
    private int eyeTargetIndex;

    private MeshRenderer HeadSphere;

    public void Awake()
    {
        Eye  = GetComponentInChildren<FieldOfView>();
        Body = GetComponent<Transform>();
        Nose = GetComponentInChildren<Nostrils>();
        Bones = GetComponent<Rigidbody>();

        noseTargetIndex = 0;
        eyeTargetIndex = 0;

        MeshRenderer[] mr = GetComponentsInChildren<MeshRenderer>();

        foreach( MeshRenderer r in mr )
        {
            if (r.tag == "eye")
            {
                HeadSphere = r;
                break;
            }
        }
    }

    public void InheritFrom( EntityController entity )
    {
        Brain = new NeuralNetwork(entity.Brain);
        Genes = new Genes(entity.Genes);
        Legs = new EightDirController(this);

        generation = (int) Brain.gen;
        variant = entity.variant;
        revision = entity.revision;

        if( Brain.isMutated )
        {
            revision++;
        }

        if( Genes.isMutated ) {
            variant++;
        }
        BaseName = entity.BaseName;
        Name = BaseName + " " + generation + "." + variant + "." + revision;

        Immortal = entity.Immortal;

        isInited = true;
    }

    public float GetSpeed() {
        return speed;
    }

    public float GetHunger()
    {
        return CurrrentFeedingTimer;
    }

    private void OnDisable()
    {
        DisableEyeLashes();
    }

    // Use this for initialization
    void Start()
    {
        if( !isInited )
        {
            Genes = new Genes();
            Brain = new NeuralNetwork(new int[ 5 ] { 5, 15, 15, 15, 2 });
            Legs = new EightDirController(this);

            Name = BaseName = Collections.Names[ Random.Range(0, Collections.Names.Count) ];
        }

        NeuStr = Brain.lineage;
        Distance = 0f;
        Consumption = 0f;

        Output = new float[ Brain.layers[ Brain.layers.Length - 1] ];

        name = "Fish " + Name;

        Age = 0;

        Energy = MaxEnergy;
        CurrrentFeedingTimer = FeedingTimer;

        displacement = 0;
        speed = 0;
        topSpeed = 0;
        guiLastPosition = lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if( enabled && ( Immortal || Energy > 0 ) ) // marked as dead doesn't matter around here
        {
            Vector3 noseInput = Vector3.zero;
            Vector3 eyeInput = Vector3.zero;

            if( Nose.visibleTargets.Count > 0 ) {
                int i = 0;
                lastNoseTartget = null;

                foreach(Transform t in Nose.visibleTargets) {
                    lastNoseTartget = t;

                    if ( noseTargetIndex == i++ ) {
                        noseTargetIndex++;
                        break;
                    }
                }
            }
            else {
                noseTargetIndex = 0;
            }

            if( Eye.visibleTargets.Count > 0 ) {
                int i = 0;
                lastEyeTarget = null;

                foreach( Transform t in Eye.visibleTargets ) {
                    lastEyeTarget = t;

                    if( eyeTargetIndex == i++ ) {
                        eyeTargetIndex++;
                        break;
                    }
                }
            }
            else {
                eyeTargetIndex = 0;
            }

            if( lastEyeTarget != null )
            {
                eyeInput = (lastEyeTarget.transform.position - Body.transform.position).normalized;
                eyeInput = transform.InverseTransformDirection(eyeInput);
            }

            if( lastNoseTartget != null )
            {
                noseInput = (lastNoseTartget.transform.position - Body.transform.position).normalized;
                noseInput = transform.InverseTransformDirection(noseInput);
            }

            float noseTargetInput = Nose.visibleTargets.Count -1;
            float eyeTargetInput  = Eye.visibleTargets.Count -1;

            Input = new float[ 5 ] {
                eyeInput.x,
                eyeInput.z,
                noseInput.x,
                noseInput.z,
                ((Energy / MaxEnergy) - 0.5f) * 2
            };

            Output = Brain.FeedForward(Input);

            Age += Time.deltaTime;

            Distance += Vector3.Distance(lastPosition, transform.position);
            displacement += Vector3.Distance(lastPosition, transform.position);
            lastPosition = transform.position;

            if (speed < 2.67f)
            {
                var mat = HeadSphere.material;
                mat.color = StillHeadColor;
                HeadSphere.material = mat;
            }
            else
            {
                var mat = HeadSphere.material;
                mat.color = MovingHeadColor;
                HeadSphere.material = mat;
            }

            if (CurrrentFeedingTimer <= 0)
            {
                UseEnergy(Time.deltaTime * 5);
            }
            else
            {
                CurrrentFeedingTimer -= Time.deltaTime;
            }
        }
        else
        {
            if( !markedAsDead )
            {
                Renderer r = GetComponent<Renderer>();

                Material m = r.material;
                m.color = DeadColor;
                r.material = m;

                markedAsDead = true;
            }

            if ( Eye.enabled ) {
                Eye.enabled = false;
            }

            if( Nose.enabled )
            {
                Nose.enabled = false;
            }
        }

        if ( lastNoseTartget != null && Nose.visibleTargets.Count == 0 ) {
            lastNoseTartget = null;
        }

        if( lastEyeTarget != null && Eye.visibleTargets.Count == 0 )
        {
            lastEyeTarget = null;
        }
    }

    void OnCollisionEnter( Collision collision )
    {
        if( Energy > 0 )
        {
            if( collision.gameObject.tag == "Fish" )
            {
                EntityController otherFish = collision.gameObject.GetComponentInParent<EntityController>();

                if( otherFish != null && !otherFish.isAlive() && !otherFish.eaten )
                {
                    //Debug.Log(Name + " ate " + otherFish.Name);

                    otherFish.enabled = false;
                    otherFish.eaten = true;
                    GainEnergy(100);
                    Consumption += 2;
                    otherFish.transform.position += new Vector3(0, -1000, 0);

                    CurrrentFeedingTimer += FeedingTimer * 2;
                }
            }

            if( collision.gameObject.tag == "Food" )
            {
                FoodController food = collision.gameObject.GetComponentInParent<FoodController>();

                if( food != null )
                {
                    //Debug.Log(Name + " ate a food");

                    GainEnergy(50);
                    Consumption += 1;

                    food.enabled = false;
                    Destroy(food);
                    Destroy(food.gameObject);
                    CurrrentFeedingTimer += FeedingTimer;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if( Energy > 0 )
        {
            speed = (float)System.Math.Round(displacement / Time.fixedDeltaTime, 2);
            displacement = 0;

            if ( speed > topSpeed ) {
                topSpeed = speed;
            }

            Legs.BabySteps(Output[0], Output[1]);
        }
    }

    private void OnDrawGizmos()
    {
        if( Energy > 0 && AllowRender )
        {
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, transform.forward * (speed + transform.localScale.z));

            if( lastNoseTartget != null )
            {
                Gizmos.color = Color.red;
                //Gizmos.DrawLine(lastNoseTartget.position, transform.position);
                Gizmos.DrawWireSphere(lastNoseTartget.position, 4f);
                Gizmos.DrawWireSphere(transform.position, Genes.Noze.range);
            }

            if( Nose.enabled && Nose.visibleTargets.Count > 0 )
            {
                foreach( Transform vt in Nose.visibleTargets )
                {
                    if( vt != null )
                    {
                        Gizmos.DrawLine(vt.position, transform.position);
                    }
                }
            }

            if( lastEyeTarget != null )
            {
                Gizmos.color = Color.yellow;
                //Gizmos.DrawLine(lastEyeTarget.position, transform.position);
                Gizmos.DrawWireSphere(lastEyeTarget.position, 13f);
                Gizmos.DrawWireSphere(transform.position, Genes.Eyes.range);
            }

            if( Eye.enabled && Eye.visibleTargets.Count > 0 )
            {
                foreach( Transform vt in Eye.visibleTargets )
                {
                    if( vt != null )
                    {
                        Gizmos.DrawLine(vt.position, transform.position);
                    }
                }
            }

            Vector3 dir = (transform.position - guiLastPosition).normalized * Genes.Legs.speed * 15;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(guiLastPosition, transform.position);

            //Gizmos.DrawRay(guiLastPosition, transform.position);
            guiLastPosition = transform.position;
        }
    }

    public void UseEnergy( float amount )
    {
        if (Immortal) {
            return;
        }

        Energy -= Mathf.Abs(amount);
    }

    public void GainEnergy(float amount) {
        if( Immortal )
        {
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

    #region ICopmare

    public int CompareTo( EntityController other )
    {
        if( other == null ) return 1;

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
        return Immortal || (!markedAsDead || Energy > 0); // yes
    }

    public float GetFittness()
    {
        return (Distance + Age + topSpeed) * (Consumption + 0.01f );
    }

    public float GetTopSpeed()
    {
        return topSpeed;
    }

    #endregion
}
