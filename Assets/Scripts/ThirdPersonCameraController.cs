using System.Collections;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    public Transform Target;
    public Camera Followingcamera;

    public Vector3 offsetPos;

    public float moveSpeed   = 5f;
    public float turnSpeed   = 10f;
    public float smoothSpeed = 0.5f;

    private Quaternion targetRotation;
    private Vector3 targetPos;
    private bool smoothRotating = false;

    private void Start()
    {
        if ( Target != null )
        {
            LookAtTarget();
        }
    }

    // Update is called once per frame
    private void Update ()
    {
        //if (enabled)
        //{
            MoveWithTarget();
            LookAtTarget();

            //Left camera pan
            if (Input.GetKeyDown(KeyCode.Q) && !smoothRotating)
            {
                StartCoroutine("RotateAroundTarget", 45);
            }

            //right camera pan
            if (Input.GetKeyDown(KeyCode.E) && !smoothRotating)
            {
                StartCoroutine("RotateAroundTarget", -45);
            }
        //}
    }

    private void MoveWithTarget()
    {
        if (Target != null)
        {
            targetPos = Target.position + offsetPos;
            Followingcamera.transform.position = Vector3.Lerp(Followingcamera.transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// use the angle to aim at the target
    /// </summary>
    private void LookAtTarget()
    {
        if (Target != null)
        {
            targetRotation = Quaternion.LookRotation(Target.position - Followingcamera.transform.position);
            Followingcamera.transform.rotation = Quaternion.Slerp(Followingcamera.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    private IEnumerator RotateAroundTarget(float angle)
    {
        Vector3 vel             = Vector3.zero;
        Vector3 targetOffsetPos = Quaternion.Euler(0, angle, 0) * offsetPos;
        float distance          = Vector3.Distance(offsetPos, targetOffsetPos);

        smoothRotating = true;

        while(distance > 0.02f)
        {
            offsetPos = Vector3.SmoothDamp(offsetPos, targetOffsetPos, ref vel, smoothSpeed);
            distance  = Vector3.Distance(offsetPos, targetOffsetPos);

            yield return null;
        }

        smoothRotating = false;
        offsetPos = targetOffsetPos;
    }
}
