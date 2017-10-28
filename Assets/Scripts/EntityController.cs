using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour, System.IComparable<EntityController>
{

    public string Name { get; private set; }

    public NeuralNetwork Brain;
    public Genes Genes;
    public EightDirController Legs;
    //public SphereCollider Nose;
    public FieldOfView Eye;
    public Nostrils Nose;
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

    public Transform lastNoseTartget;
    public Transform eyeLastTarget;

    public void Awake()
    {
        Eye  = GetComponentInChildren<FieldOfView>();
        Body = GetComponent<Transform>();
        Nose = GetComponentInChildren<Nostrils>();
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
            Brain = new NeuralNetwork(new int[ 5 ] { 6, 64, 48, 64, 8 });
            Legs = new EightDirController(this);

            Name = Collections.Names[ Random.Range(0, Collections.Names.Count) ];
        }

        name = "Fish " + Name;
        NeuStr = Brain.lineage;
        Distance = 0f;
        Consumption = 0f;

        Output = new float[ Brain.layers[ Brain.layers.Length - 1] ];

        Age = 0;
        //Energy = 1000;

        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if( Energy > 0 )
        {
            Vector3 noseInput = Vector3.zero;
            Vector3 eyeInput = Vector3.zero;

            if( Nose.visibleTargets.Count > 1 )
            {
                noseInput = (Body.transform.position - Nose.visibleTargets[ 1 ].transform.position).normalized;

                foreach(Transform t in Nose.visibleTargets ) {
                    if (t != Body) {
                        lastNoseTartget = t;
                        break;
                    }
                }               
                //print("IS" + noseInput);
            }
            else {
                lastNoseTartget = null;
            }

            if( Eye.visibleTargets.Count > 1 )
            {
                eyeInput = (Body.transform.position - Eye.visibleTargets[ 1 ].transform.position).normalized;

                foreach( Transform t in Eye.visibleTargets )
                {
                    if( t != Body)
                    {
                        eyeLastTarget = t;
                        break;
                    }
                }
            }
            else {
                eyeLastTarget = null;
            }

            float noseTargetInput = Nose.visibleTargets.Count > 1 ? 1 :Nose.visibleTargets.Count < 1 ? -1 : 0;
            float eyeTargetInput = Eye.visibleTargets.Count > 1 ? 1 :Eye.visibleTargets.Count == 1 ? -1 : 0;

            Input = new float[ 6 ] {
                noseTargetInput,
                eyeTargetInput,
                eyeInput.x,
                eyeInput.z,
                noseInput.x,
                noseInput.z
            };

            Output = Brain.FeedForward(Input);

            Age += Time.deltaTime;
            Energy -= Mathf.Abs(Time.deltaTime);
            Distance += Vector3.Distance(lastPosition, transform.position);
            lastPosition = transform.position;
        }
        else
        {
            if( !markedAsDead )
            {
                Renderer r = GetComponent<Renderer>();

                Material m = r.material;
                m.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
                r.material = m;

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

    void OnTriggerExit( Collider other )
    {
    }

    private void FixedUpdate()
    {
        if( Energy > 0 )
        {
            bool yes = true;
            
            if( lastNoseTartget != null && Output[ 0 ] + Output[ 1 ] > 0.75f ) //Genes.smth.smth
            {
                Legs.MoveTowardsTarget(lastNoseTartget);
                //Vector3 dirToTarget = (lastNoseTartget.position - Body.position).normalized;
                //Legs.HandleInput(dirToTarget.x, dirToTarget.z);
            }
            else {
                yes = false;
            }

            if( eyeLastTarget != null && Output[ 2 ] + Output[ 3 ] > 0.75f ) //Genes.smth.smth
            {
                Legs.MoveTowardsTarget(eyeLastTarget);
                //Vector3 dirToTarget = (eyeLastTarget.position - Body.position).normalized;
                //Legs.HandleInput(dirToTarget.x, dirToTarget.z);
            }
            else {
                yes = false;
            }

            if ( !yes ) {
                Legs.Nudge();
            }
        }
    }

    private void LateUpdate()
    {
        if( Energy > 0 )
        {

            
        }
        else
        {
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

            if( lastNoseTartget != null )
            {
                //Gizmos.DrawWireSphere(transform.position, Genes.Nose.range);
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
