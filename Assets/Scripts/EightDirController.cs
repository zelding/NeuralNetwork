﻿﻿using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 1. 8 dir movement
/// 2. stop and face camera
/// </summary>
public class EightDirController {

    public float velocity  = 5f;
    public float turnSpeed = 3f;

    public LayerMask[] obstacleLayer;

    private EntityController entity;
    private Rigidbody body;

    Vector3 currentVelocity;
    float smoothInputMagnitude;
    float smoothMoveVelocity;
    float smoothMoveTime = .1f;

    private float angle;
    private Quaternion targetRotation;

    private List<Vector3> moveBuffer;

    public EightDirController( EntityController entity )
    {
        this.entity = entity;
        body = entity.GetComponentInChildren<Rigidbody>();

        velocity  = entity.Genes.Legs.speed;
        turnSpeed = entity.Genes.Legs.turnSpeed;

        moveBuffer = new List<Vector3>();

        currentVelocity = Vector3.zero;
    }

    public void HandleInput(float x, float y, float t)
    {
        Vector3 inputDirection = new Vector3(x, 0, y).normalized;

        float inputMagnitude = inputDirection.magnitude;
        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMagnitude, ref smoothMoveVelocity, smoothMoveTime);

        float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
        angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * turnSpeed * inputMagnitude);

        currentVelocity = entity.transform.forward * velocity * smoothInputMagnitude * t;
        /*
        if (moveBuffer.Count >= 3  || (moveBuffer.Count > 1 && moveBuffer[moveBuffer.Count - 1] == moveBuffer[moveBuffer.Count - 2])) {

            Vector3 inputDirection = calcAvg(moveBuffer).normalized;

            float inputMagnitude = inputDirection.magnitude;
            smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMagnitude, ref smoothMoveVelocity, smoothMoveTime);

            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
            angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * turnSpeed * inputMagnitude);

            currentVelocity = entity.transform.forward * velocity * smoothInputMagnitude * t;

            moveBuffer.Clear();
        }
        else {
            Vector3 inputDirection = new Vector3(x, 0, y).normalized;

            moveBuffer.Add(inputDirection);
        }*/
    }

    public void Move()
    {
        body.MoveRotation(Quaternion.Euler(Vector3.up * angle));
        body.MovePosition(body.position + currentVelocity * Time.deltaTime);
    }

    public Vector3 calcAvg(List<Vector3> list)
    {
        if (list.Count == 0) {
            return Vector3.zero;
        }

        float x = 0;
        float y = 0;
        float z = 0;

        foreach (Vector3 v in list) {
            x += v.x;
            y += v.y;
            z += v.z;
        }

        float k = 1.0f / Mathf.Sqrt(x * x + y * y + z * z);
        return new Vector3(x * k, y * k, z * k);
    }
}
