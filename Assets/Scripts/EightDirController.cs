﻿﻿using UnityEngine;

/// <summary>
/// 1. 8 dir movement
/// 2. stop and face camera
/// </summary>
[System.Serializable]
public class EightDirController {

    public float velocity  = 5f;
    public float turnSpeed = 3f;
    //public Camera myCamera;

    public LayerMask[] obstacleLayer;

    private EntityController entity;
    private Rigidbody body;

    Vector3 currentVelocity;

    private Vector2 input;
    private float angle;
    private Quaternion targetRotation;

    public EightDirController( EntityController entity, Camera camera = null )
    {
        this.entity = entity;
        body = entity.GetComponentInChildren<Rigidbody>();

        velocity = entity.Genes.Legs.speed;
        turnSpeed = entity.Genes.Legs.turnSpeed;
    }

    public bool HandleInput( float x, float y )
    {
        input = new Vector2(x, y);

        if( Mathf.Abs(input.x) < 0.1f && Mathf.Abs(input.y) < 0.1f )
        {
            entity.Bones.velocity = Vector3.zero;
            return false;
        }

        CalculateDirection();
        Rotate();
        Move();

        return true;
    }

    public void Nudge()
    {
        body.AddForce(Random.onUnitSphere * velocity / 10f, ForceMode.Impulse);
        //body.transform.Translate(Vector3.forward);
    }

    public void MoveTowardsTarget( Transform target )
    {
        Vector3 dirToTarget = (target.position - body.position).normalized;

        float angle = Vector3.Angle(body.transform.forward, dirToTarget);

        if( angle <= 20f )
        {
            /* float dstToTarget = Vector3.Distance (body.position, target.position);

             entity.transform.LookAt(target);*/

            body.transform.Rotate(Vector3.up, angle);



            /* body.transform.Translate(dirToTarget * velocity / 10f);*/

        }

        body.AddForce(Vector3.forward * velocity, ForceMode.Force);
        entity.UseEnergy(velocity / 10f);

    }

    public void BabySteps(float x, float y)
    {
        Vector3 mousePos = new Vector3(x, 0, y).normalized;
        entity.transform.LookAt(mousePos + Vector3.up * entity.transform.position.y);
        currentVelocity = new Vector3(x, 0, y).normalized * velocity;

        body.MovePosition(entity.transform.position + currentVelocity * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Dir relative to the cameras rotation
    /// </summary>
    private void CalculateDirection()
    {
        angle  = Mathf.Atan2(input.x, input.y);
        angle *= Mathf.Rad2Deg; //convert to degrees
        angle += entity.transform.rotation.eulerAngles.y;
    }

    /// <summary>
    /// Rotate towards the calc angle
    /// </summary>
    private void Rotate()
    {
       //Vector3 dirToTarget = (target.position - body.position).normalized;

        targetRotation            = Quaternion.Euler(0, angle, 0);
        Quaternion rotationVector = Quaternion.Slerp(entity.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        //body.AddRelativeTorque(rotationVector.eulerAngles, ForceMode.Impulse);

        //body.MoveRotation(rotationVector);
        entity.transform.rotation = rotationVector;

        entity.UseEnergy(turnSpeed / 10f);
    }

    /// <summary>
    /// 
    /// </summary>
    private void Move()
    {
        //entity.transform.position += entity.transform.forward * velocity * Time.deltaTime;
        body.AddRelativeForce(entity.transform.forward * velocity, ForceMode.Impulse);

        entity.UseEnergy(velocity / 10f);
    }
}
