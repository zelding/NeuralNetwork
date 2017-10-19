using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour {

    public readonly string Name;

    [SerializeField]
    public NeuralNetwork Brain;
    public EightDirController Legs;
    public Camera myCamera;

    [SerializeField]
    public float[] Output { get; private set; }

    public float[] Input { private get; set; }

    public string NeuStr;

    public float speed = 18.334f;

    private float gravity = 14f;
    private float verticalVelocity;
    private CharacterController controller;

    [SerializeField]
    private float age      = 0f;
    [SerializeField]
    private float energy   = 100f;

    public bool isAlive()
    {
        return energy > 0;
    }

	// Use this for initialization
	void Start ()
    {
        Brain      = new NeuralNetwork(new int[4] { 4, 6, 6, 4 });
        Legs       = new EightDirController(this, myCamera);
        controller = GetComponent<CharacterController>();

        NeuStr = Brain.lineage;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (energy > 0)
        {
            Input = new float[4] {
                Random.Range(-10f, 10f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            };

            if (controller.isGrounded)
            {
                verticalVelocity = -gravity * Time.deltaTime;
                transform.position += transform.forward * speed * Output[0] * Time.deltaTime;
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
            }

            Output = Brain.FeedForward(Input);

            Vector3 moveVector = new Vector3(0, verticalVelocity, 0);
            controller.Move(moveVector * Time.deltaTime);

            Legs.HandleInput(Output[0] * speed, Output[1] * speed);

            age += Time.deltaTime;
            energy -= Time.deltaTime;
        }

        Debug.Log(Output[0] + "," + Output[1]);
    }

    private void OnDrawGizmos()
    {
        float distance = 1.00f;

        Gizmos.DrawRay(transform.position, transform.forward * (distance + transform.localScale.z));
    }
}
