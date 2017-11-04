﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodController : MonoBehaviour {

    public string ID { get; private set; }

    public const float massFactor = 0.67f;

    [Range(1.5f, 200)]
    public float StandardMaxRadius = 30;

    public float MaxRadius = 30;

    [Range(1.0f, 100)]
    public float closeRange = 7f;

    [Range(1.0f, 100)]
    public float veryCloseRange = 5f;

    public LayerMask foodMask;

    public float Cycle { get; private set; }
    public float CurrentRadius { get; private set; }

    public bool EnableBubleing = false;
    [Range(0.34f, 5f)]
    public float Frequency;

    public int Nearby;
    public int Close;
    
    Coroutine Bubbling;

    [HideInInspector]
    public SphereCollider stench;

    private Rigidbody body;

    private Vector3 lastDirection;
    private int nearbyFood;
    private int veryCloseFood;

    private void Awake()
    {
        SphereCollider[] stenches = GetComponents<SphereCollider>();

        if (stenches.Length > 0) {
            foreach( SphereCollider s in stenches) {
                if(s.isTrigger) {
                    stench = s;
                    break;
                }
            }
        }

        body = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    void Start () {
        ID = Random.Range(-10000f, 10000).ToString();

        stench.radius = StandardMaxRadius / transform.localScale.x;

        CurrentRadius = stench.radius;
        MaxRadius = StandardMaxRadius;

        Cycle = 0;
        nearbyFood = 0;
        veryCloseFood = 0;

        if( EnableBubleing )
        {
            Bubbling = StartCoroutine(Buble(Time.fixedDeltaTime));
        }

        lastDirection = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if( !enabled )
        {
            if( Bubbling != null )
            {
                StopCoroutine(Bubbling);
            }
        }
        else {
            CountNearByFood();
        }
    }

    void OnTriggerEnter( Collider col )
    {
        FoodController target = col.GetComponent<FoodController>();

        if ( target != null && target.ID != ID ) 
        {
            float dst = Vector3.Distance(transform.position, col.transform.position);

            if ( dst > closeRange && dst < MaxRadius)
            {
                lastDirection = (col.transform.position - transform.position).normalized;
            }
        }
    }

    private void FixedUpdate()
    {
        if( enabled )
        {
            if( Bubbling != null )
            {
                Bubbling = StartCoroutine(Buble(Time.fixedDeltaTime));
            }

            if(lastDirection != Vector3.zero ) 
            {
                body.AddForce(lastDirection * (nearbyFood + 0.01f), ForceMode.Impulse);
                lastDirection = Vector3.zero;
            }

            MaxRadius = StandardMaxRadius * (1 + (1 + veryCloseFood) * massFactor);
            Nearby = nearbyFood;
            Close = veryCloseFood;
        }

        CurrentRadius = stench.radius;
    }

    void CountNearByFood()
    {
        veryCloseFood = nearbyFood = 0;
        Collider[] targetsInViewRadius = Physics.OverlapSphere(body.position, MaxRadius, foodMask);

        for( int i = 0; i < targetsInViewRadius.Length; i++ )
        {
            FoodController target = targetsInViewRadius[i].GetComponent<FoodController>();

            if( target != null && target.ID != ID )
            {
                float dstToTarget = Vector3.Distance(body.position, target.transform.position);

                if( dstToTarget >= closeRange )
                {
                    nearbyFood++;
                }

                if( dstToTarget <= veryCloseRange && dstToTarget >= 1 )
                {
                    veryCloseFood++;
                }
            }
        }

        nearbyFood = Mathf.RoundToInt(nearbyFood * 0.5f);
        veryCloseFood = Mathf.RoundToInt(veryCloseFood * 0.5f);
    }

    IEnumerator Buble(float delay) {
        while (true) {
            if( Cycle < Frequency ) {
                stench.radius = Evaluate(Cycle) * (MaxRadius / transform.localScale.x);
                Cycle += Time.fixedDeltaTime;
            }
            else {
                Cycle = 0;
            }

            Bubbling = null;

            yield return new WaitForSeconds(delay);
        }
    }

    static float Evaluate(float value){
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
