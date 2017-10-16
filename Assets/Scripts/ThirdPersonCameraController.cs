using System.Collections;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    public Transform target;

    public Vector3 offsetPos;

    public float moveSpeed   = 5f;
    public float turnSpeed   = 10f;
    public float smoothSpeed = 0.5f;

    private Quaternion targetRotation;
    private Vector3 targetPos;
    private bool smoothRotating = false;

    // Update is called once per frame
    private void Update ()
    {
        if (isActiveAndEnabled)
        {
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
        }
    }

    private void MoveWithTarget()
    {
        targetPos          = target.position + offsetPos;
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// use the angle to aim at the target
    /// </summary>
    private void LookAtTarget()
    {
        targetRotation     = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
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
