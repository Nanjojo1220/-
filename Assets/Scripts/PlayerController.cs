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
    [Header("移動")]
    [SerializeField] private float walkSpeed = 5f;   // 歩き速度
    [SerializeField] private float dashMultiplier = 2f; // ダッシュ倍率
    [SerializeField] private float rotationSpeed = 10f; // 回転速度

    [Header("ジャンプ")]
    [SerializeField] private float jumpPower = 16f;  // ジャンプ初速
    [SerializeField] private float jumpCutPower = 0.5f; // 小ジャンプ倍率（キー離した時に掛ける）

    [Header("重力")]
    [SerializeField] private float fallMultiplier = 2.5f; // 落下時の重力倍率
    [SerializeField] private float lowJumpMultiplier = 2f; // 短押し時の重力倍率

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
        // カメラ基準の前後左右
        Vector3 camForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)).normalized;

        // 入力
        Vector3 input = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) input += camForward;
        if (Input.GetKey(KeyCode.S)) input -= camForward;
        if (Input.GetKey(KeyCode.A)) input -= camRight;
        if (Input.GetKey(KeyCode.D)) input += camRight;

        // 速度
        float currentSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) currentSpeed *= dashMultiplier;

        Vector3 velocity = rb.velocity;
        Vector3 move = input.normalized * currentSpeed;

        velocity.x = move.x;
        velocity.z = move.z;
        rb.velocity = velocity;

        // 回転
        if (input != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(input, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        // アニメーション（任意）
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

        // 上昇中 → スペースを離したら低めジャンプに
        if (vel.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            vel.y += Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
        // 落下中 → 重力を強める
        else if (vel.y < 0)
        {
            vel.y += Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        rb.velocity = vel;
    }

    private void HandleJump()
    {
        // ジャンプ開始
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
            isGrounded = false;
        }

        // 小ジャンプ（キーを離したら上昇をカット）
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
