using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour {

    public readonly string Name;

    public NeuralNetwork Brain;

    public float[] Output { get; private set; }

    public float[] Input { private get; set; }

    public string NeuStr;

    public float speed = 8.334f;

    private float gravity = 14f;
    private float verticalVelocity;
    private CharacterController controller;

	// Use this for initialization
	void Start ()
    {
        Brain = new NeuralNetwork(new int[4] { 4, 6, 6, 4 });
        controller = GetComponent<CharacterController>();

        NeuStr = Brain.lineage;
    }
	
	// Update is called once per frame
	void Update ()
    {
        Input = new float[4] {
                Random.Range(-10f, 10f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            };

        if ( controller.isGrounded )
        {
            verticalVelocity = -gravity * Time.deltaTime;
            transform.position += transform.forward * speed * Output[0] * Time.deltaTime;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        //Output = Brain.FeedForward(Input);

        //Vector3 moveVector = new Vector3(0, verticalVelocity, 0);
        //controller.Move(moveVector * Time.deltaTime);

        //Debug.Log(Input[0].ToString() +" => "+ Output[0].ToString());
    }

    private void OnDrawGizmos()
    {
        float distance = 1.00f;

        Gizmos.DrawRay(transform.position, transform.forward * (distance + transform.localScale.z));
    }

    private void OnGUI()
    {
        ///Brain.Mutate();
        //Brain = new NeuralNetwork(Brain);
        //Brain.Mutate();
    }
}
