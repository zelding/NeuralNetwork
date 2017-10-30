﻿using System.Collections;
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
    public Rigidbody Bones;

    [Header("Hmmmmmm")]
    public bool Immortal = false;

    [Range(500f, 5000f)]
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
    public Transform lastEyeTarget;

    public void Awake()
    {
        Eye  = GetComponentInChildren<FieldOfView>();
        Body = GetComponent<Transform>();
        Nose = GetComponentInChildren<Nostrils>();
        Bones = GetComponent<Rigidbody>();
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

        Immortal = entity.Immortal;

        isInited = true;
    }

    public bool isAlive()
    {
        return Immortal || ( !markedAsDead || Energy > 0 ); // yes
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
        if( enabled && ( Immortal || Energy > 0 ) ) // marked as dead doesn't matter around here
        {
            Vector3 noseInput = Vector3.zero;
            Vector3 eyeInput = Vector3.zero;

            if( Nose.visibleTargets.Count > 0 ) {
                foreach(Transform t in Nose.visibleTargets ) {
                    if (t != Body) {
                        lastNoseTartget = t;
                        break;
                    }
                }
            }

            if( Eye.visibleTargets.Count > 0 ) {
                foreach( Transform t in Eye.visibleTargets ) {
                    if( t != Body) {
                        lastEyeTarget = t;
                        break;
                    }
                }
            }

            if( lastEyeTarget != null )
            {
                eyeInput = (lastEyeTarget.transform.position - Body.transform.position);
            }

            if( lastNoseTartget != null )
            {
                noseInput = (lastNoseTartget.transform.position - Body.transform.position);
            }

            float noseTargetInput = Nose.visibleTargets.Count -1;
            float eyeTargetInput = Eye.visibleTargets.Count -1;

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
            UseEnergy(Time.deltaTime);
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
                    Consumption += 10;
                }
            }

            if( collision.gameObject.tag == "Food" )
            {
                FoodController food = collision.gameObject.GetComponentInParent<FoodController>();

                if( food != null )
                {
                    //Debug.Log(Name + " ate a food");

                    GainEnergy(10);
                    Consumption += 1;

                    food.enabled = false;
                    Destroy(food.gameObject);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if( Energy > 0 )
        {
            bool yes = true;
            
            if( lastNoseTartget != null && (Mathf.Abs(Output[ 0 ]) > 0 || Mathf.Abs(Output[ 1 ]) > 0) ) //Genes.smth.smth
            {
                //Legs.MoveTowardsTarget(lastNoseTartget);
                //Vector3 dirToTarget = (lastNoseTartget.position - Body.position).normalized;
                //Legs.HandleInput(NeuralNetwork.Normalize(Output[ 0 ]), NeuralNetwork.Normalize(Output[ 1 ]));
                Legs.BabySteps(NeuralNetwork.Normalize(Output[ 0 ]), NeuralNetwork.Normalize(Output[ 1 ]));
            }
            else {
                yes = false;
            }

            if( lastEyeTarget != null && (Mathf.Abs(Output[ 2 ]) > 0 || Mathf.Abs(Output[ 3 ]) > 0) ) //Genes.smth.smth
            {
                //Legs.MoveTowardsTarget(eyeLastTarget);
                //Vector3 dirToTarget = (eyeLastTarget.position - Body.position).normalized;
                //Legs.HandleInput(dirToTarget.x, dirToTarget.z);
                //Legs.HandleInput(NeuralNetwork.Normalize(Output[ 2 ]), NeuralNetwork.Normalize(Output[ 3 ]));
                Legs.BabySteps(NeuralNetwork.Normalize(Output[ 2 ]), NeuralNetwork.Normalize(Output[ 3 ]));
            }
            else {
                yes = false;
            }

            if ( !yes ) {
                
            }

            //Legs.HandleInput(0, 1);
            UseEnergy(0.3f);
        }
    }

    private void OnDrawGizmos()
    {
        if( Energy > 0 )
        {
            float distance = 10.00f;

            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, transform.forward * (distance + transform.localScale.z));

            if( lastNoseTartget != null )
            {
                Gizmos.color = Color.red;//new Color(0.5f, 1, 0, 1f);
                Gizmos.DrawLine(lastNoseTartget.position, transform.position);
            }

            if( lastEyeTarget != null )
            {
                Gizmos.color = Color.yellow;//new Color(1, 0.5f, 0, 1f);
                Gizmos.DrawLine(lastEyeTarget.position, transform.position);
            }
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

    #region ICopmare

    public int CompareTo( EntityController other )
    {
        if( other == null ) return 1;

        float sum = Distance + Age;

        return sum.CompareTo(other.Distance + other.Age);
    }

    #endregion
}
