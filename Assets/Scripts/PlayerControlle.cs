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
        // --- ���͏��� ---
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)).normalized;

        moveDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDir += cameraForward;
        if (Input.GetKey(KeyCode.S)) moveDir -= cameraForward;
        if (Input.GetKey(KeyCode.A)) moveDir -= cameraRight;
        if (Input.GetKey(KeyCode.D)) moveDir += cameraRight;

        moveDir = moveDir.normalized * moveSpeedIn;

        // �A�j���[�V��������
        animator.SetBool("walk", moveDir.magnitude > 0.1f);

        // --- �W�����v���� ---
        if (Input.GetKeyDown(KeyCode.Space) && playerStatus == Status.GROUND)
        {
            verticalVel = jumpPower;
            playerStatus = Status.UP;
        }
    }

    void FixedUpdate()
    {
        // --- �n�ʔ��� ---
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f; // �����ォ�画��
        bool isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance + 0.1f);

        // --- �d�͏��� ---
        if (playerStatus != Status.GROUND)
        {
            if (verticalVel > 0 && !Input.GetKey(KeyCode.Space))
            {
                // ���W�����v�p�̋����d��
                verticalVel -= lowJumpGravity * Time.fixedDeltaTime;
            }
            else
            {
                // �ʏ�d��
                verticalVel -= gravity * Time.fixedDeltaTime;
            }
        }

        // --- �n�ʂɒ��n������ ---
        if (isGrounded && verticalVel <= 0f)
        {
            verticalVel = -2f; // �n�ʂɉ����t����
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

        // --- ���ۂ̈ړ� ---
        Vector3 finalVelocity = new Vector3(moveDir.x, verticalVel, moveDir.z);
        playerRb.velocity = finalVelocity;

        // --- �����̍X�V�i�ړ������������j ---
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

