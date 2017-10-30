﻿using System.Collections;
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
        if (enabled)
        {
            MoveWithTarget();
            LookAtTarget();

            if ( Input.GetKeyDown(KeyCode.Space) && !smoothRotating ) {
                targetRotation = Quaternion.LookRotation(Target.forward);
                Followingcamera.transform.rotation = Quaternion.Slerp(Followingcamera.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            if ( Input.mouseScrollDelta.magnitude != 0 ) {
                offsetPos += new Vector3(0, Input.mouseScrollDelta.y, Input.mouseScrollDelta.y);
            }
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
        }
    }

    /// <summary>
    /// use the angle to aim at the target
    /// </summary>
    private void LookAtTarget()
    {
        if (Target != null)
        {
            //targetRotation = Quaternion.LookRotation(Target.position - Followingcamera.transform.position);
            //Followingcamera.transform.rotation = Quaternion.Slerp(Followingcamera.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            float angle = Mathf.SmoothDampAngle(Followingcamera.transform.rotation.eulerAngles.y, Target.rotation.eulerAngles.y, ref turnSpeed, 0.3f);
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
