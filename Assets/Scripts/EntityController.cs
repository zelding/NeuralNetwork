using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour, System.IComparable<EntityController>
{

    public string Name { get; private set; }

    public NeuralNetwork Brain;
    public Genes Genes;
    public EightDirController Legs;
    public SphereCollider Nose;
    public FieldOfView Eye;
    public Transform Body;

    [Range(500, 5000)]
    public float Energy;

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

    private Quaternion lastNoseTartget;

    public void Awake()
    {
        Eye = GetComponentInChildren<FieldOfView>();
        Nose = GetComponentInChildren<SphereCollider>();
    }

    public void InheritFrom( EntityController entity )
    {
        Brain = new NeuralNetwork(entity.Brain);
        Genes = new Genes(entity.Genes);
        Legs = new EightDirController(this);

        Name = entity.Name + entity.Name[ entity.Name.Length - 1 ];

        if( Genes.isMutated )
        {
            Name = Name + Collections.Names[ Random.Range(0, Collections.Names.Count) ][ 0 ];
        }
        
        if( Brain.isMutated )
        {
            Name += Collections.Names[ Random.Range(0, Collections.Names.Count) ][ 1 ];
        }

        if ( Genes.isMutated && Brain.isMutated ) 
        {
            Name = Name.Contains("X-") ? Name.Replace("X-", "") : "X-" + Name;
        }

        isInited = true;
    }

    public bool isAlive()
    {
        return !markedAsDead || Energy > 0;
    }

    // Use this for initialization
    void Start()
    {
        if( !isInited )
        {
            Genes = new Genes();
            Brain = new NeuralNetwork(new int[ 4 ] { 3, 32, 32, 4 });
            Legs = new EightDirController(this);

            Name = Collections.Names[ Random.Range(0, Collections.Names.Count) ];
        }

        name = "Fish " + Name;
        NeuStr = Brain.lineage;
        Distance = 0f;
        Consumption = 0f;

        Age = 0;
        Energy = 1000;

        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if( Energy > 0 )
        {
            float noseInput = 0f;
            float eyeInput = 0f;

            if( lastNoseTartget != Quaternion.identity )
            {
                noseInput = lastNoseTartget.eulerAngles.y;
            }

            if( Eye.visibleTargets.Count > 0 )
            {
                eyeInput = Eye.visibleTargets[ 0 ].eulerAngles.y;
            }

            Input = new float[ 3 ] {
                noseInput,
                Age,
                eyeInput
            };

            Output = Brain.FeedForward(Input);

            Age += Time.deltaTime;
            Energy -= Mathf.Abs(Time.deltaTime);
            Distance += Vector3.Distance(lastPosition, transform.position);
            lastPosition = transform.position;
        }
        else
        {
            lastNoseTartget = Quaternion.identity;

            if( !markedAsDead )
            {
                Renderer[] rs = GetComponentsInChildren<Renderer>();
                foreach( Renderer r in rs )
                {
                    Material m = r.material;
                    m.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
                    r.material = m;
                }

                markedAsDead = true;
            }
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
                    Debug.Log(Name + " ate " + otherFish.Name);

                    otherFish.enabled = false;
                    otherFish.eaten = true;
                    Energy += 200;
                    Consumption += 200;
                }
            }
        }
    }

    void OnTriggerEnter( Collider other )
    {

        if( Energy > 0 )
        {
            if( other.gameObject != gameObject && other.transform.root.tag == "Fish" )
            {
                lastNoseTartget = Quaternion.RotateTowards(transform.rotation, other.transform.rotation, 45f);
            }
        }
    }

    void OnTriggerExit( Collider other )
    {
        lastNoseTartget = Quaternion.identity;
    }

    private void FixedUpdate()
    {

    }

    private void LateUpdate()
    {
        if( Energy > 0 )
        {
            if( !Legs.HandleInput(Input[ 0 ], Input[ 1 ]) )
            {
                UseEnergy(2f);
            }
        }
        else {
            Eye.enabled = false;
            Nose.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        if( Energy > 0 )
        {
            float distance = 5.00f;

            Gizmos.DrawRay(transform.position, transform.forward * (distance + transform.localScale.z));

            if( lastNoseTartget != Quaternion.identity )
            {
                Gizmos.DrawWireSphere(transform.position, 60);
            }
        }
    }

    public void UseEnergy( float amount )
    {
        Energy -= Mathf.Abs(amount);
    }

    #region ICopmare

    public int CompareTo( EntityController other )
    {
        if( other == null ) return 1;

        float sum = Distance + Age;

        return sum.CompareTo(other.Distance + other.Age);
    }

    #endregion
}
