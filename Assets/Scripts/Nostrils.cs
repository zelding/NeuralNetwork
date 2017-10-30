﻿﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class Nostrils : MonoBehaviour
{
    public Color TargetColor;
    public Color RangeColor;

    public float viewRadius;
    [Range(0,360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public bool allowRender;

    public List<Transform> visibleTargets = new List<Transform>();

    public float meshResolution = 10f;
    public int edgeResolveIterations = 4;
    public float edgeDstThreshold = 0.67f;

    public Transform Body;

    public MeshFilter viewMeshFilter;

    //--------------------------------//

    SphereCollider Sphere;
    Genes.AMutateable Chromosome;

    float range;
    float resolution;
    int maxTargets;

    private void Awake()
    {
        Sphere = GetComponent<SphereCollider>();
        Body   = GetComponentInParent<Transform>();

        TargetColor = Color.cyan;
        RangeColor = new Color(0, 0.6f, 0.6f, 0.6f);
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

    void OnDrawGizmos()
    {
        /*if( enabled && visibleTargets.Count > 0 )
        {
            Gizmos.color = TargetColor;
            foreach( Transform vt in visibleTargets )
            {
                Gizmos.DrawLine(vt.position, Body.position);
            }

            Gizmos.color = RangeColor;
            Gizmos.DrawWireSphere(transform.position, viewRadius);
        }*/
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

