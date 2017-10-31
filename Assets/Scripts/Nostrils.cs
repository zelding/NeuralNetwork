﻿﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Nostrils : MonoBehaviour
{
    public float viewRadius;
    [Range(0,360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public bool allowRender;

    public List<Transform> visibleTargets = new List<Transform>();

    public Transform Body;

    Genes.Movement Chromosome;

    float range;
    float resolution;
    int maxTargets;

    private void Awake()
    {
        Body   = GetComponentInParent<Transform>();
    }

    private void Start()
    {
        resolution = Mathf.Clamp01(resolution);
        maxTargets = Mathf.Clamp(maxTargets, 1, 5);
        range = Mathf.Clamp(range, 20, 200);

        if( targetMask != 0 && obstacleMask != 0 )
        {
            StartCoroutine("FindTargetsWithDelay", Time.fixedDeltaTime);
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(Body.position, viewRadius, targetMask);

        for( int i = 0; i < targetsInViewRadius.Length; i++ )
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - Body.position).normalized;

            float dstToTarget = Vector3.Distance (target.position, Body.position);

            if( dstToTarget <= viewRadius ) // ?
            {
                if( !Physics.Raycast(Body.position, dirToTarget, dstToTarget, obstacleMask) )
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    IEnumerator FindTargetsWithDelay( float delay )
    {
        while( enabled )
        {
            FindVisibleTargets();
            yield return new WaitForSeconds(delay);
        }
    }
}

