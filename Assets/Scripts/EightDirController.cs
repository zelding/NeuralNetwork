using UnityEngine;

/// <summary>
/// 1. 8 dir movement
/// 2. stop and face camera
/// </summary>
public class EightDirController : MonoBehaviour {

    public float velocity  = 5f;
    public float turnSpeed = 10f;
    public Camera myCamera;

    private Vector2 input;
    private float angle;
    private Quaternion targetRotation;
    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = myCamera.transform;
    }

    private void Update()
    {
        GetInput();

        if( Mathf.Abs(input.x) < 1 && Mathf.Abs(input.y) < 1 )
        {
            return;
        }

        CalculateDirection();
        Rotate();
        Move();
    }

    /// <summary>
    /// 
    /// </summary>
    private void GetInput()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
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
        targetRotation     = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 
    /// </summary>
    private void Move()
    {
        transform.position += transform.forward * velocity * Time.deltaTime;
    }
}
