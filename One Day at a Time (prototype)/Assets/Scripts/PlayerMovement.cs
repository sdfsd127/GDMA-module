using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float playerMoveSpeed;
    [SerializeField] private float playerMinMoveSpeed;
    [SerializeField] private float playerMaxMoveSpeed;

    private void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // DIR Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 dir = transform.right * x + transform.forward * z;
        dir.y = rb.velocity.y;
        rb.velocity = dir * playerMoveSpeed;
    }
}
