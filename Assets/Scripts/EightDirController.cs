using UnityEngine;

/// <summary>
/// 1. 8 dir movement
/// 2. stop and face camera
/// </summary>
public class EightDirController {

    public float velocity  = 5f;
    public float turnSpeed = 10f;
    public Camera myCamera;

    private Entity entity;

    private Vector2 input;
    private float angle;
    private Quaternion targetRotation;
    private Transform cameraTransform;

    public EightDirController(Entity entity, Camera camera)
    {
        this.entity     = entity;
        myCamera        = camera;
        cameraTransform = myCamera.transform;

        velocity  = entity.Genes.Legs.speed;
        turnSpeed = entity.Genes.Legs.turnSpeed;
    }

    public void HandleInput(float x, float y)
    {
        input = new Vector2(x, y);

        if( Mathf.Abs(input.x) < 1 && Mathf.Abs(input.y) < 1 )
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
        angle += cameraTransform.eulerAngles.y;

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
