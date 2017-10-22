using UnityEngine;

/// <summary>
/// 1. 8 dir movement
/// 2. stop and face camera
/// </summary>
public class EightDirController {

    public float velocity  = 5f;
    public float turnSpeed = 10f;
    //public Camera myCamera;

    private EntityController entity;

    private Vector2 input;
    private float angle;
    private Quaternion targetRotation;
    private Transform cameraTransform;

    public EightDirController(EntityController entity, Camera camera = null)
    {
        this.entity     = entity;

        velocity  = entity.Genes.Legs.speed;
        turnSpeed = entity.Genes.Legs.turnSpeed;
    }

    public void HandleInput(float x, float y)
    {
        input = new Vector2(x, y);

        if( Mathf.Abs(input.x) < 0.85f && Mathf.Abs(input.y) < 0.85f )
        {
            return;
        }

        CalculateDirection();
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
        targetRotation            = Quaternion.Euler(0, angle, 0);
        entity.transform.rotation = Quaternion.Slerp(entity.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 
    /// </summary>
    private void Move()
    {
        entity.transform.position += entity.transform.forward * velocity * Time.deltaTime;
    }

    private void LateUpdate()
    {
        Rotate();
        Move();
    }
}
