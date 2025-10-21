using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlle : MonoBehaviour
{
    [SerializeField] float moveSpeedIn = 5f; // 地上の移動速度
    [SerializeField] float jumpPower = 16f;  // ジャンプ初速
    [SerializeField] float gravity = 60f;    // 重力の強さ
    [SerializeField] float lowJumpGravity = 90f; // 小ジャンプ用の強め重力
    [SerializeField] float groundCheckDistance = 0.2f; // 地面判定距離

    Animator animator;
    Rigidbody playerRb;

    Status playerStatus = Status.GROUND;

    Vector3 moveDir;   // XZ方向の入力
    float verticalVel; // Y方向の速度（重力・ジャンプ用）

    void Start()
    {
        animator = GetComponent<Animator>();
        playerRb = GetComponent<Rigidbody>();
        playerRb.useGravity = false; // Unityの重力はOFF
    }

    void Update()
    {
        // === 入力処理 ===
        float h = Input.GetAxis("Horizontal"); // キーボードA/D, コントローラー左スティックX
        float v = Input.GetAxis("Vertical");   // キーボードW/S, コントローラー左スティックY

        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)).normalized;

        moveDir = (cameraForward * v + cameraRight * h).normalized * moveSpeedIn;

        // === アニメーション制御 ===
        animator.SetBool("walk", moveDir.magnitude > 0.1f);

        // === ジャンプ入力 ===
        bool jumpPressed = Input.GetButtonDown("Jump"); // Spaceキー or コントローラーのAボタン
        if (jumpPressed && playerStatus == Status.GROUND)
        {
            verticalVel = jumpPower;
            playerStatus = Status.UP;
        }
    }

    void FixedUpdate()
    {
        // === 地面判定 ===
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        bool isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance + 0.1f);

        // === 重力処理 ===
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

        // === 着地判定 ===
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

        // === 移動処理 ===
        Vector3 finalVelocity = new Vector3(moveDir.x, verticalVel, moveDir.z);
        playerRb.velocity = finalVelocity;

        // === 向き制御 ===
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

