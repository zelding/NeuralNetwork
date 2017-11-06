using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodGravity : MonoBehaviour {

    const float G = 1f;

    public static List<FoodGravity> Attractors;

    public Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if( Attractors == null ) {
            Attractors = new List<FoodGravity>();
        }

        Attractors.Add(this);
    }

    private void OnDisable()
    {
        Attractors.Remove(this);
    }

    void FixedUpdate() 
    {
        foreach(FoodGravity f in Attractors) {
            if( f != this ) {
                Attract(f);
            }
        }
    }

    void Attract(FoodGravity f)
    {
        Rigidbody rbToAttract = f.rb;

        Vector3 direction = rb.position - rbToAttract.position;
        float distance = direction.magnitude;
        float sqDistance = Vector3.SqrMagnitude(direction);

        if( distance == 0f ) {
            return;
        }

        float forceMagnitude = G * (rb.mass * rbToAttract.mass) / sqDistance;
        Vector3 force = direction.normalized * forceMagnitude;

        rbToAttract.AddForce(force);
    }
}
