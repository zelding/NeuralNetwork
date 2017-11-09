using System.Collections;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    public Transform Target;
    public Camera Followingcamera;

    public Vector3 offsetPos;
    public Quaternion offsetRotation;

    public float scrollspeed_y = 3f;
    public float scrollspeed_z = 3f;

    public float moveSpeed   = 5f;
    public float turnSpeed   = 10f;
    public float smoothSpeed = 0.5f;

    private Quaternion targetRotation;
    private Vector3 targetPos;
    private bool smoothRotating = false;

    private Vector3 turning;

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
        if (enabled)
        {
            MoveWithTarget();

            if ( Input.mouseScrollDelta.magnitude != 0 ) {
                offsetPos += new Vector3(0, Input.mouseScrollDelta.y * scrollspeed_y, -Input.mouseScrollDelta.y * scrollspeed_z);
            }

            LookAtTarget();
        }
        else
        {
            smoothRotating = false;
        }
    }

    private void MoveWithTarget()
    {
        if (Target != null)
        {
            Vector3 destination = Target.rotation * offsetPos;

            destination += Target.position;

            Followingcamera.transform.position = destination;

            /*Vector3 dest = Target.position;
            dest += Quaternion.Euler(67f,0, 0) * Target.rotation * -Vector3.forward * 100;

            transform.position = Vector3.SmoothDamp(transform.position, dest, ref turning, 0.3f);*/
        }
    }

    /// <summary>
    /// use the angle to aim at the target
    /// </summary>
    private void LookAtTarget()
    {
        if (Target != null)
        {
            float angle = Mathf.SmoothDampAngle(Followingcamera.transform.rotation.eulerAngles.y, Target.rotation.eulerAngles.y, ref turnSpeed, 0.3f);
            //Quaternion targetRotation = Quaternion.Euler(Followingcamera.transform.rotation.x, angle, 0);

            //Followingcamera.transform.rotation = Quaternion.Slerp(Followingcamera.transform.rotation, Target.rotation * Followingcamera.transform.rotation, Time.fixedDeltaTime);

            Followingcamera.transform.rotation = Quaternion.Euler(Followingcamera.transform.rotation.x, angle, 0);
        }
    }

    private IEnumerator RotateAroundTarget(float angle)
    {
        Vector3 vel             = Vector3.zero;
        Vector3 targetOffsetPos = Quaternion.Euler(0, angle, 0) * offsetPos;
        float distance          = Vector3.Distance(offsetPos, targetOffsetPos);

        smoothRotating = true;

        while(distance > 0.01f)
        {
            offsetPos = Vector3.SmoothDamp(offsetPos, targetOffsetPos, ref vel, smoothSpeed);
            distance  = Vector3.Distance(offsetPos, targetOffsetPos);

            yield return null;
        }

        smoothRotating = false;
        offsetPos = targetOffsetPos;
    }
}
