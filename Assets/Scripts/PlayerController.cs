using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;



using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�ړ�")]
    [SerializeField] private float walkSpeed = 5f;   // �������x
    [SerializeField] private float dashMultiplier = 2f; // �_�b�V���{��
    [SerializeField] private float rotationSpeed = 10f; // ��]���x

    [Header("�W�����v")]
    [SerializeField] private float jumpPower = 16f;  // �W�����v����
    [SerializeField] private float jumpCutPower = 0.5f; // ���W�����v�{���i�L�[���������Ɋ|����j

    [Header("�d��")]
    [SerializeField] private float fallMultiplier = 2.5f; // �������̏d�͔{��
    [SerializeField] private float lowJumpMultiplier = 2f; // �Z�������̏d�͔{��

    private Rigidbody rb;
    private Animator animator;

    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        // �J������̑O�㍶�E
        Vector3 camForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)).normalized;

        // ����
        Vector3 input = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) input += camForward;
        if (Input.GetKey(KeyCode.S)) input -= camForward;
        if (Input.GetKey(KeyCode.A)) input -= camRight;
        if (Input.GetKey(KeyCode.D)) input += camRight;

        // ���x
        float currentSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) currentSpeed *= dashMultiplier;

        Vector3 velocity = rb.velocity;
        Vector3 move = input.normalized * currentSpeed;

        velocity.x = move.x;
        velocity.z = move.z;
        rb.velocity = velocity;

        // ��]
        if (input != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(input, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        // �A�j���[�V�����i�C�Ӂj
        if (animator != null)
        {
            bool isMoving = input.magnitude > 0.1f;
            animator.SetBool("walk", isMoving && !Input.GetKey(KeyCode.LeftShift));
            animator.SetBool("Run", isMoving && Input.GetKey(KeyCode.LeftShift));
        }
    }

    void FixedUpdate()
    {
        Vector3 vel = rb.velocity;

        // �㏸�� �� �X�y�[�X�𗣂������߃W�����v��
        if (vel.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            vel.y += Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
        // ������ �� �d�͂����߂�
        else if (vel.y < 0)
        {
            vel.y += Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        rb.velocity = vel;
    }

    private void HandleJump()
    {
        // �W�����v�J�n
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
            isGrounded = false;
        }

        // ���W�����v�i�L�[�𗣂�����㏸���J�b�g�j
        if (Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * jumpCutPower, rb.velocity.z);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
