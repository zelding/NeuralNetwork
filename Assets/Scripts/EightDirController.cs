﻿﻿using UnityEngine;

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

    public EightDirController( EntityController entity )
    {
        this.entity = entity;
        body = entity.GetComponentInChildren<Rigidbody>();

        velocity  = entity.Genes.Legs.speed;
        turnSpeed = entity.Genes.Legs.turnSpeed;

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
    }

    public void Move()
    {
        body.MoveRotation(Quaternion.Euler(Vector3.up * angle));
        body.MovePosition(body.position + currentVelocity * Time.deltaTime);
    }
}
