using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlle : MonoBehaviour
{
    [SerializeField] float moveSpeedIn = 5f; // �n��̈ړ����x
    [SerializeField] float jumpPower = 16f;  // �W�����v����
    [SerializeField] float gravity = 60f;    // �d�͂̋���
    [SerializeField] float lowJumpGravity = 90f; // ���W�����v�p�̋��ߏd��
    [SerializeField] float groundCheckDistance = 0.2f; // �n�ʔ��苗��

    Animator animator;
    Rigidbody playerRb;

    Status playerStatus = Status.GROUND;

    Vector3 moveDir;   // XZ�����̓���
    float verticalVel; // Y�����̑��x�i�d�́E�W�����v�p�j

    void Start()
    {
        animator = GetComponent<Animator>();
        playerRb = GetComponent<Rigidbody>();
        playerRb.useGravity = false; // Unity�̏d�͂�OFF
    }

    void Update()
    {
        // === ���͏��� ===
        float h = Input.GetAxis("Horizontal"); // �L�[�{�[�hA/D, �R���g���[���[���X�e�B�b�NX
        float v = Input.GetAxis("Vertical");   // �L�[�{�[�hW/S, �R���g���[���[���X�e�B�b�NY

        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)).normalized;

        moveDir = (cameraForward * v + cameraRight * h).normalized * moveSpeedIn;

        // === �A�j���[�V�������� ===
        animator.SetBool("walk", moveDir.magnitude > 0.1f);

        // === �W�����v���� ===
        bool jumpPressed = Input.GetButtonDown("Jump"); // Space�L�[ or �R���g���[���[��A�{�^��
        if (jumpPressed && playerStatus == Status.GROUND)
        {
            verticalVel = jumpPower;
            playerStatus = Status.UP;
        }
    }

    void FixedUpdate()
    {
        // === �n�ʔ��� ===
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        bool isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance + 0.1f);

        // === �d�͏��� ===
        if (playerStatus != Status.GROUND)
        {
            if (verticalVel > 0 && !Input.GetButton("Jump"))
            {
                verticalVel -= lowJumpGravity * Time.fixedDeltaTime;
            }
            else
            {
                verticalVel -= gravity * Time.fixedDeltaTime;
            }
        }

        // === ���n���� ===
        if (isGrounded && verticalVel <= 0f)
        {
            verticalVel = -2f;
            playerStatus = Status.GROUND;
        }
        else if (verticalVel > 0f)
        {
            playerStatus = Status.UP;
        }
        else
        {
            playerStatus = Status.DOWN;
        }

        // === �ړ����� ===
        Vector3 finalVelocity = new Vector3(moveDir.x, verticalVel, moveDir.z);
        playerRb.velocity = finalVelocity;

        // === �������� ===
        Vector3 flatMove = new Vector3(moveDir.x, 0, moveDir.z);
        if (flatMove.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatMove, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * (groundCheckDistance + 0.1f));
    }
}

