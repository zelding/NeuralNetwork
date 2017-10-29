using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class Nostrils : FieldOfView
{
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

    void LateUpdate()
    {

        if( enabled )
        {

        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(Body.position, range, targetMask);

        for( int i = 0; i < targetsInViewRadius.Length; i++ )
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - Body.position).normalized;

            float dstToTarget = Vector3.Distance (target.position, Body.position);

            if( dstToTarget != range ) // ?
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

    private void OnDrawGizmos()
    {
        if( enabled && visibleTargets.Count > 0 )
        {
            Gizmos.color = Color.cyan;
            foreach( Transform vt in visibleTargets ) {
                Gizmos.DrawLine(vt.transform.position, Body.transform.position);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}

