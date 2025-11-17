using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Collections.Specialized;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float sensitivity = 3.0f;
    [SerializeField] private float minYAngle = -30f;
    [SerializeField] private float maxYAngle = 60f;
    [SerializeField] private float followSmooth = 10f;
    [SerializeField] private float cameraDistance = 6f;
    [SerializeField] private float lookOffsetY = 1.5f;
    [SerializeField] private float verticalOffset = 2.0f;

    private float yaw;
    private float pitch;

    // Dead Zone 
    private float deadZone = 0.2f;

    [SerializeField] private LayerMask collisionMask; // 衝突させたいレイヤー（例：Default, Environment）
    [SerializeField] private float cameraRadius = 0.3f; // カメラがめり込まないための半径

    private Vector3 camVelocity = Vector3.zero;
    private Renderer lastObstacleRenderer = null;
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

    private float smoothYaw;
    private float smoothPitch;

    [SerializeField] private float rotateSmoothTime = 0.05f;
    private float yawVelocity;
    private float pitchVelocity;

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            UnityEngine.Debug.LogError("CameraController: Playerが未設定です.");
            enabled = false;
            return;
        }

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");


        float rightX = Input.GetAxis("RightStickHorizontal");
        float rightY = Input.GetAxis("RightStickVertical");


        if (Mathf.Abs(rightX) < deadZone) rightX = 0f;
        if (Mathf.Abs(rightY) < deadZone) rightY = 0f;


        float inputX = mouseX * sensitivity + rightX * sensitivity;
        float inputY = mouseY * sensitivity + rightY * sensitivity;


        yaw += inputX;
        pitch -= inputY;
        float clampedPitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);

        smoothYaw = Mathf.SmoothDampAngle(smoothYaw, yaw, ref yawVelocity, rotateSmoothTime);
        smoothPitch = Mathf.SmoothDampAngle(smoothPitch, pitch, ref pitchVelocity, rotateSmoothTime);

        pitch = clampedPitch;

        Quaternion rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0);

        Vector3 targetCenter = player.transform.position + Vector3.up * verticalOffset;
        Vector3 desiredPos = targetCenter - rotation * Vector3.forward * cameraDistance;


        // --- キャラ → カメラへ Ray を飛ばす（遮蔽物チェック） ---
        RaycastHit obstacleHit;

        float currentDistance = cameraDistance;

        if (Physics.Raycast(targetCenter, (desiredPos - targetCenter).normalized,
                            out obstacleHit, cameraDistance, collisionMask))
        {
            GameObject hitObj = obstacleHit.collider.gameObject;
            Renderer r = obstacleHit.collider.GetComponent<Renderer>();

            // ★ Object レイヤーなら → 透過
            if (hitObj.layer == LayerMask.NameToLayer("Object"))
            {
                // 前回の障害物を元に戻す
                if (lastObstacleRenderer != null && lastObstacleRenderer != r)
                {
                    if (originalColors.ContainsKey(lastObstacleRenderer))
                        lastObstacleRenderer.material.color = originalColors[lastObstacleRenderer];
                }

                if (r != null)
                {
                    if (!originalColors.ContainsKey(r))
                        originalColors[r] = r.material.color;

                    Color c = originalColors[r];
                    c.a = 0.3f;
                    r.material.color = c;

                    lastObstacleRenderer = r;
                }
            }
            else
            {
                // Object 以外 → 透過しない、カメラを寄せる
                currentDistance = obstacleHit.distance - 0.3f;

                // 透過を戻す
                if (lastObstacleRenderer != null)
                {
                    if (originalColors.ContainsKey(lastObstacleRenderer))
                        lastObstacleRenderer.material.color = originalColors[lastObstacleRenderer];

                    lastObstacleRenderer = null;
                }
            }
        }
        else
        {
            // 遮蔽物なし → 透過を戻す
            if (lastObstacleRenderer != null)
            {
                if (originalColors.ContainsKey(lastObstacleRenderer))
                    lastObstacleRenderer.material.color = originalColors[lastObstacleRenderer];

                lastObstacleRenderer = null;
            }
        }

        // ★ カメラの最終位置を再計算（ズームイン対応）
        desiredPos = targetCenter - rotation * Vector3.forward * currentDistance;




        // --- カメラ位置追従（SmoothDampで揺れを抑制） ---
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref camVelocity,
            0.2f  // ← 小さくするほどキビキビ、大きいほど安定
        );

        // --- 回転（Yaw/Pitch から生成） ---
        Quaternion targetRot = Quaternion.Euler(smoothPitch, smoothYaw, 0);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * followSmooth
        );



    }

    // Update is called once per frame
    void Update()
    {

        float rh = Input.GetAxis("RightStickHorizontal");
        float rv = Input.GetAxis("RightStickVertical");

    }
}