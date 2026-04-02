using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    Rigidbody rb;

    Vector3 moveVectorInput;
    Vector3 moveDirection;
    Vector3 rotationDirection;

    [SerializeField] float speed = 3f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] Camera targetCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void AddmoveVectorInput(Vector3 moveVector)
    {
        moveVectorInput = moveVector.normalized;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        if (targetCamera == null) return;

        Vector3 cameraForward = targetCamera.transform.forward;
        Vector3 cameraRight = targetCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        moveDirection = cameraForward * moveVectorInput.z;
        moveDirection += cameraRight * moveVectorInput.x;

        Vector3 newPosition = rb.position + moveDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        moveVectorInput = Vector3.zero;
    }

    private void HandleRotation()
    {
        if (moveDirection.magnitude > 0f)
        {
            rotationDirection = moveDirection;
        }

        if (rotationDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);
            Quaternion rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(rotation);
        }
    }
}