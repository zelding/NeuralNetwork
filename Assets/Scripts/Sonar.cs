using System.Collections;
using UnityEngine;

public class Sonar : MonoBehaviour
{
    public LayerMask obstacleLayer;

    private EntityController entity;

    private float wallInFront;
    private float wallInLeft;
    private float wallInRight;

    Coroutine scanning;

    // Use this for initialization
    void Start()
    {
        entity   = GetComponentInParent<EntityController>();
        scanning = StartCoroutine("FindTargetsWithDelay", 0.25f);
    }

    // Update is called once per frame
    void Update()
    {
        DetectObstaces();
    }

    public Vector3 GetData()
    {
        return new Vector3(wallInLeft, wallInFront, wallInRight);
    }

    IEnumerator FindTargetsWithDelay( float delay )
    {
        while( enabled ) {
            DetectObstaces();
            yield return new WaitForSeconds(delay);
        }
    }

    private void DetectObstaces()
    {
        RaycastHit hit;
        Vector3 rightVector = Quaternion.AngleAxis(45, transform.up) * transform.forward;
        Vector3 leftVector = Quaternion.AngleAxis(-45, transform.up) * transform.forward;

        if( Physics.Raycast(transform.position, transform.forward, out hit, entity.Genes.Ears.range, obstacleLayer) ) {
            wallInFront = hit.distance;
        }
        else {
            wallInFront = 0;
        }

        if( Physics.Raycast(transform.position, leftVector, out hit, entity.Genes.Ears.range, obstacleLayer) ) {
            wallInLeft = hit.distance;
        }
        else {
            wallInLeft = 0;
        }

        if( Physics.Raycast(transform.position, rightVector, out hit, entity.Genes.Ears.range, obstacleLayer) ) {
            wallInRight = hit.distance;
        }
        else {
            wallInRight = 0;
        }
    }
}
