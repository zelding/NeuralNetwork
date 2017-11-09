﻿﻿﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour
{
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public bool allowRender;

    public Transform Body;

    public MeshFilter viewMeshFilter;

    public List<Transform> visibleTargets = new List<Transform>();

    float viewRadius;
    float viewAngle;
    float meshResolution;
    int edgeResolveIterations;
    float edgeDstThreshold;
    
    Mesh viewMesh;
    EntityController entity;
    Coroutine scanning;

    void Awake()
    {
        Body = GetComponentInParent<Transform>();
        viewMeshFilter = GetComponent<MeshFilter>();
        entity = GetComponentInParent<EntityController>();
    }

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        meshResolution = 5;
        edgeResolveIterations = 4;
        edgeDstThreshold = 0.67f;

        viewAngle = entity.Genes.Eyes.angle;
        viewRadius = entity.Genes.Eyes.range;

        scanning = StartCoroutine("FindTargetsWithDelay", 0.25f);
    }

    IEnumerator FindTargetsWithDelay( float delay )
    {
        while( enabled )
        {
            FindVisibleTargets();
            yield return new WaitForSeconds(delay);
        }
    }

    void LateUpdate()
    {
        if( enabled )
        {
            DrawFieldOfView();
        }
        else
        {
            if( scanning != null )
            {
                StopCoroutine(scanning);
            }
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(Body.position, viewRadius, targetMask);

        for( int i = 0; i < targetsInViewRadius.Length; i++ )
        {
            Transform target = targetsInViewRadius[i].transform;

            if( target == transform.parent.transform ) {
                continue;
            }

            Vector3 dirToTarget = (target.position - Body.position).normalized;

            if( Vector3.Angle(Body.forward, dirToTarget) < viewAngle / 2 )
            {
                float dstToTarget = Vector3.Distance (Body.position, target.position);

                if( !Physics.Raycast(Body.position, dirToTarget, dstToTarget, obstacleMask) )
                {
                    EntityController te = targetsInViewRadius[i].GetComponent<EntityController>();

                    if (te == null || !te.isAlive())
                    {
                        visibleTargets.Add(target);
                    }
                }
            }
        }
    }

    void DrawFieldOfView()
    {
        if( allowRender )
        {
            int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
            float stepAngleSize = viewAngle / stepCount;
            List<Vector3> viewPoints = new List<Vector3> ();
            ViewCastInfo oldViewCast = new ViewCastInfo ();
            for( int i = 0; i <= stepCount; i++ )
            {
                float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
                ViewCastInfo newViewCast = ViewCast (angle);

                if( i > 0 )
                {
                    bool edgeDstThresholdExceeded = Mathf.Abs (oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                    if( oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded) )
                    {

                        EdgeInfo edge = FindEdge (oldViewCast, newViewCast);

                        if( edge.pointA != Vector3.zero )
                        {
                            viewPoints.Add(edge.pointA);
                        }

                        if( edge.pointB != Vector3.zero )
                        {
                            viewPoints.Add(edge.pointB);
                        }
                    }

                }


                viewPoints.Add(newViewCast.point);
                oldViewCast = newViewCast;
            }

            int vertexCount = viewPoints.Count + 1;
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles    = new int[(vertexCount-2) * 3];

            vertices[ 0 ] = Vector3.zero;
            for( int i = 0; i < vertexCount - 1; i++ )
            {
                vertices[ i + 1 ] = transform.InverseTransformPoint(viewPoints[ i ]);

                if( i < vertexCount - 2 )
                {
                    triangles[ i * 3 ] = 0;
                    triangles[ i * 3 + 1 ] = i + 1;
                    triangles[ i * 3 + 2 ] = i + 2;
                }
            }

            viewMesh.Clear();

            viewMesh.vertices = vertices;
            viewMesh.triangles = triangles;
            viewMesh.RecalculateNormals();
        }
        else if( viewMesh.vertexCount > 0 ) {
            viewMesh.Clear();
        }
    }

    EdgeInfo FindEdge( ViewCastInfo minViewCast, ViewCastInfo maxViewCast )
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for( int i = 0; i < edgeResolveIterations; i++ )
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast (angle);

            bool edgeDstThresholdExceeded = Mathf.Abs (minViewCast.dst - newViewCast.dst) > edgeDstThreshold;

            if( newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded )
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    ViewCastInfo ViewCast( float globalAngle )
    {
        Vector3 dir = DirFromAngle (globalAngle, true);
        RaycastHit hit;

        if( Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask) )
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    public Vector3 DirFromAngle( float angleInDegrees, bool angleIsGlobal )
    {
        if( !angleIsGlobal )
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo( bool _hit, Vector3 _point, float _dst, float _angle )
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo( Vector3 _pointA, Vector3 _pointB )
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

    public struct TargetData
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly float RelativeHealth;
        public Vector3 Color; // (as<=>) typeof(Color)?
        public readonly string Name;

        public TargetData( Vector3 position, Quaternion rotation, float relativeHealth, Vector3 color, string name )
        {
            Position = position;
            Rotation = rotation;
            RelativeHealth = relativeHealth;
            Color = color;
            Name = name;
        }
    }
}
