using UnityEngine;

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

    private Vector2 input;
    private float angle;
    private Quaternion targetRotation;

    public EightDirController(EntityController entity, Camera camera = null)
    {
        this.entity     = entity;
        body = entity.GetComponentInChildren<Rigidbody>();

        velocity  = entity.Genes.Legs.speed;
        turnSpeed = entity.Genes.Legs.turnSpeed;
    }

    public bool HandleInput(float x, float y)
    {
        input = new Vector2(x, y);

        if( Mathf.Abs(input.x) < 0.67f && Mathf.Abs(input.y) < 0.67f )
        {
            return false;
        }

        CalculateDirection();
        Rotate();
        Move();

        return true;
    }

    public void Nudge()
    {
        body.AddForce(Vector3.forward * -velocity, ForceMode.Impulse);
    }

    public void MoveTowardsTarget(Transform target)
    {
        Vector3 dirToTarget = (target.position - body.position).normalized;

       // body.AddTorque(Quaternion.LookRotation(slerpToTarget, Vector3.up).eulerAngles.normalized * turnSpeed, ForceMode.Force);

        if( Vector3.Angle(body.transform.forward, dirToTarget) <= 270f )
        {
            float dstToTarget = Vector3.Distance (body.position, target.position);

            entity.transform.LookAt(target);

            body.AddForce(Vector3.forward * velocity, ForceMode.Force);
            entity.UseEnergy(velocity / 10f);
        }

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
        body.AddRelativeTorque(rotationVector.eulerAngles, ForceMode.Impulse);

        //body.MoveRotation(rotationVector);

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
