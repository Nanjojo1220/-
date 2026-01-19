using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動")]
    [SerializeField] private float walkSpeed = 5f;   // 歩き速度
    [SerializeField] private float dashMultiplier = 2f; // ダッシュ倍率
    [SerializeField] private float rotationSpeed = 10f; // 回転速度

    [Header("AIM制御（追加）")]
    [SerializeField] private float aimMoveMultiplier = 0.4f; // AIM中の移動速度倍率
    [SerializeField] private float aimRotateSpeed = 8f;      // AIM中の向き補正速度
    public bool IsAiming { get; set; }                        // Camera側から設定

    [Header("ジャンプ")]
    [SerializeField] private float jumpPower = 8f;
    [SerializeField] private float jumpCutPower = 0.5f;
    [SerializeField] private float jumpCutGravity = 35f;

    [Header("重力")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    private Rigidbody rb;
    private Animator animator;

    [Header("地面判定")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.3f;
    private bool isGrounded = true;

    [Header("カメラ操作")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float cameraRotateSpeed = 120f;


    [SerializeField] private float deathY = -10f;
    private bool isDead = false;
   

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (cameraPivot == null && Camera.main != null)
        {
            cameraPivot = Camera.main.transform.parent != null
                ? Camera.main.transform.parent
                : Camera.main.transform;
        }
    }

    void Update()
    {
        CheckGrounded();

        HandleCameraRotation();
        HandleMovement();
        HandleJump();

        if (!isDead && transform.position.y < deathY)
        {
            isDead = true;
        }
    }

    public void ResetDeathFlag()
    {
        isDead = false;
    }

    private void HandleMovement()
    {
        Camera cam = Camera.main;
        Vector3 camForward = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cam.transform.right, new Vector3(1, 0, 1)).normalized;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 input = (camForward * vertical + camRight * horizontal);

        // ダッシュ判定（AIM中は不可）
        bool isDash = !IsAiming && (Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Fire3"));

        float currentSpeed = walkSpeed;

        if (IsAiming)
        {
            currentSpeed *= aimMoveMultiplier;
        }
        else if (isDash)
        {
            currentSpeed *= dashMultiplier;
        }

        Vector3 velocity = rb.velocity;
        Vector3 move = input.normalized * currentSpeed;

        RaycastHit hit;
        if (move.sqrMagnitude > 0.01f &&
            Physics.Raycast(transform.position + Vector3.up * 0.5f, move.normalized, out hit, 0.6f))
        {
            if (!hit.collider.CompareTag("Ground"))
            {
                Vector3 wallNormal = hit.normal;
                move = Vector3.ProjectOnPlane(move, wallNormal).normalized * currentSpeed;
            }
        }

        velocity.x = move.x;
        velocity.z = move.z;
        rb.velocity = velocity;

        // ===== 回転処理 =====
        if (IsAiming)
        {
            // BOTW風：カメラ方向へ寄せる
            Vector3 camDir = camForward;
            if (camDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(camDir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    Time.deltaTime * aimRotateSpeed
                );
            }
        }
        else if (input.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(input);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotationSpeed
            );
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

        float rightX = Input.GetAxis("RightStickHorizontal");
        float rightY = Input.GetAxis("RightStickVertical");

        float rotX = rightX * cameraRotateSpeed * Time.deltaTime;
        float rotY = -rightY * cameraRotateSpeed * Time.deltaTime;

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
        bool jumpPressed = Input.GetButtonDown("Jump");
        bool jumpHeldNow = Input.GetButton("Jump");

        if (jumpPressed && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
            isGrounded = false;
        }

        if (!jumpHeldNow && rb.velocity.y > 0f)
        {
            rb.AddForce(Vector3.down * jumpCutGravity, ForceMode.Acceleration);
        }
    }

    private void CheckGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundMask);
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


