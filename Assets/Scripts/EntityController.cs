using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour , System.IComparable<EntityController>
{

    public string Name { get; private set; }

    public NeuralNetwork Brain;
    public Genes Genes;
    public EightDirController Legs;

    public float[] Output { get; private set; }
    public float[] Input { get; set; }

    public string NeuStr;

    public float speed = 18.334f;

    public float Age { get; private set; }
    public float Energy { get; private set; }

    public void InheritFrom(EntityController entity)
    {
        Brain      = new NeuralNetwork(entity.Brain);
        Genes      = new Genes(entity.Genes);
        Legs       = new EightDirController(entity);

        Name = entity.Name + " Jr.";
    }

    public bool isAlive()
    {
        return Energy > 0;
    }

	// Use this for initialization
	void Start ()
    {
        Age    = 0;
        Energy = 1000;

        Genes      = new Genes();
        Brain      = new NeuralNetwork(new int[4] { 4, 64, 64, 4 });
        Legs       = new EightDirController(this);

        NeuStr = Brain.lineage;

        Name = Collections.Names[ Random.Range( 0, Collections.Names.Count ) ];
        name = "Entity - " + Name;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Energy > 0)
        {
            Input = new float[4] {
                Random.Range(-10f, 10f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            };

            Output = Brain.FeedForward(Input);

            Age += Time.deltaTime;
            Energy -= Time.deltaTime;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Collided with: " + collision.gameObject.name);
    }

    private void FixedUpdate()
    {
        
    }

    private void LateUpdate()
    {
        if (Energy > 0)
        {
            Legs.HandleInput(Output[0], Output[1]);
        }
    }

    private void OnDrawGizmos()
    {
        float distance = 5.00f;

        Gizmos.DrawRay(transform.position, transform.forward * (distance + transform.localScale.z));
    }

    public void UseEnergy(float amount)
    {
        Energy -= amount;
    }

    #region ICopmare

    public int CompareTo(EntityController other)
    {
        if (other == null) return 1;

        return Energy.CompareTo(other.Energy);
    }

    #endregion
}
