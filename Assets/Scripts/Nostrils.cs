﻿﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Nostrils : MonoBehaviour
{
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public bool allowRender;

    public Transform Body;

    public List<Transform> visibleTargets = new List<Transform>();

    float viewRadius;

    EntityController entity;
    Coroutine scanning;

    private void Awake()
    {
        Body   = GetComponentInParent<Transform>();
        entity = GetComponentInParent<EntityController>();
    }

    private void Start()
    {
        viewRadius = entity.Genes.Noze.range;

        if( targetMask != 0 && obstacleMask != 0 )
        {
            scanning = StartCoroutine("FindTargetsWithDelay", Time.fixedDeltaTime);
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

