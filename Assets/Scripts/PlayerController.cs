using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動")]
    [SerializeField] private float walkSpeed = 5f;   // 歩き速度
    [SerializeField] private float dashMultiplier = 2f; // ダッシュ倍率
    [SerializeField] private float rotationSpeed = 10f; // 回転速度

    [Header("ジャンプ")]
    [SerializeField] private float jumpPower = 16f;  // ジャンプ初速
    [SerializeField] private float jumpCutPower = 0.5f; // 小ジャンプ倍率

    [Header("重力")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded = true;

    //  右スティックカメラ制御用
    [Header("カメラ操作")]
    [SerializeField] private Transform cameraPivot;  // カメラの親（ターゲット）
    [SerializeField] private float cameraRotateSpeed = 120f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // カメラPivotが未設定なら、自動でメインカメラの親を探す
        if (cameraPivot == null && Camera.main != null)
        {
            cameraPivot = Camera.main.transform.parent != null ? Camera.main.transform.parent : Camera.main.transform;
        }
    }

    void Update()
    {
        HandleCameraRotation();
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        Camera cam = Camera.main;
        Vector3 camForward = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cam.transform.right, new Vector3(1, 0, 1)).normalized;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 input = (camForward * vertical + camRight * horizontal);

        //ダッシュは Shift か L3（ジョイスティック押し込み）
        bool isDash = Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Fire3");

        float currentSpeed = walkSpeed;
        if (isDash) currentSpeed *= dashMultiplier;

        Vector3 velocity = rb.velocity;
        Vector3 move = input.normalized * currentSpeed;

        velocity.x = move.x;
        velocity.z = move.z;
        rb.velocity = velocity;

        // 回転処理
        if (input.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(input);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        // アニメーション
        if (animator != null)
        {
            bool isMoving = input.magnitude > 0.1f;
            animator.SetBool("walk", isMoving && !isDash);
            animator.SetBool("Run", isMoving && isDash);
        }
    }

    private void HandleCameraRotation()
    {
        if (cameraPivot == null) return;

        //右スティック入力を取得（InputManagerに設定が必要）
        float rightX = Input.GetAxis("RightStickHorizontal");
        float rightY = Input.GetAxis("RightStickVertical");

        // スティック感度を調整
        float rotX = rightX * cameraRotateSpeed * Time.deltaTime;
        float rotY = -rightY * cameraRotateSpeed * Time.deltaTime;

        // カメラをプレイヤー中心に回転
        cameraPivot.RotateAround(transform.position, Vector3.up, rotX);
        cameraPivot.RotateAround(transform.position, cameraPivot.right, rotY);
    }

    void FixedUpdate()
    {
        Vector3 vel = rb.velocity;

        if (vel.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            vel.y += Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
        else if (vel.y < 0)
        {
            vel.y += Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        rb.velocity = vel;
    }

    private void HandleJump()
    {
        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
            isGrounded = false;
        }

        if ((Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.Space)) && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * jumpCutPower, rb.velocity.z);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}


