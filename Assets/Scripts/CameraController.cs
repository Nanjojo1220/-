using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float minYAngle = -30f;
    [SerializeField] private float maxYAngle = 60f;
    [SerializeField] private float followSmooth = 10f;
    [SerializeField] private float cameraDistance = 6f;
    [SerializeField] private float lookOffsetY = 1.5f;
    [SerializeField] private float verticalOffset = 2.0f;

    [Header("Aim")]
    [SerializeField] private float normalSensitivity = 3.0f;
    [SerializeField] private float aimSensitivity = 1.2f;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float aimFOV = 40f;
    [SerializeField] private float fovSmooth = 10f;

    [Header("Aim Pitch Limit")]
    [SerializeField] private float normalMinY = -30f;
    [SerializeField] private float normalMaxY = 60f;

    [SerializeField] private float aimMinY = -80f;  // ほぼ真下
    [SerializeField] private float aimMaxY = 80f;   // ほぼ真上

    [Header("Reticle Offset")]
    [SerializeField] private float aimLookOffsetY = 1.8f; // 頭より上
    [SerializeField] private float normalLookOffsetY = 1.5f;


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

    public GameObject reticle;


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

        bool isAiming = Input.GetKey(KeyCode.JoystickButton6); // L2

        float currentSensitivity = isAiming ? aimSensitivity : normalSensitivity;

        float inputX = (mouseX + rightX) * currentSensitivity;
        float inputY = (mouseY + rightY) * currentSensitivity;



        yaw += inputX;
        pitch -= inputY;

        float minPitch = isAiming ? aimMinY : normalMinY;
        float maxPitch = isAiming ? aimMaxY : normalMaxY;

        float clampedPitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        pitch = clampedPitch;

        smoothYaw = Mathf.SmoothDampAngle(smoothYaw, yaw, ref yawVelocity, rotateSmoothTime);
        smoothPitch = Mathf.SmoothDampAngle(smoothPitch, pitch, ref pitchVelocity, rotateSmoothTime);

        pitch = clampedPitch;

        Quaternion rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0);

        float lookY = isAiming ? aimLookOffsetY : normalLookOffsetY;
        Vector3 targetCenter = player.transform.position + Vector3.up * lookY;


        // ① まず理想のカメラ位置を計算
        Vector3 desiredPos =
            targetCenter
            - rotation * Vector3.forward * cameraDistance;

        RaycastHit obstacleHit;

        // ② その方向に SphereCast
        Vector3 dir = (desiredPos - targetCenter).normalized;
        float currentDistance = cameraDistance;

        if (Physics.SphereCast(
                targetCenter,
                cameraRadius,
                dir,
                out obstacleHit,
                cameraDistance,
                collisionMask
            ))
        {
            GameObject hitObj = obstacleHit.collider.gameObject;

            // Object レイヤーは透過
            if (hitObj.layer == LayerMask.NameToLayer("Object"))
            {
                Renderer r = hitObj.GetComponent<Renderer>();

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
                // 壁など → カメラを寄せる
                currentDistance = obstacleHit.distance - cameraRadius;
                currentDistance = Mathf.Max(currentDistance, 0.5f);

                // 透過解除
                if (lastObstacleRenderer != null &&
                    originalColors.ContainsKey(lastObstacleRenderer))
                {
                    lastObstacleRenderer.material.color =
                        originalColors[lastObstacleRenderer];
                    lastObstacleRenderer = null;
                }
            }
        }
        else
        {
            // 何も当たっていない → 透過解除
            if (lastObstacleRenderer != null &&
                originalColors.ContainsKey(lastObstacleRenderer))
            {
                lastObstacleRenderer.material.color =
                    originalColors[lastObstacleRenderer];
                lastObstacleRenderer = null;
            }
        }

        // ③ 最終位置
        desiredPos =
            targetCenter
            - rotation * Vector3.forward * currentDistance;




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


        Camera cam = GetComponent<Camera>();
        float targetFOV = isAiming ? aimFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFOV,
            Time.deltaTime * fovSmooth
   
        );



    }

    // Update is called once per frame
    void Update()
    {
        bool isAiming = Input.GetKey(KeyCode.JoystickButton6);
        reticle.SetActive(isAiming);

        float rh = Input.GetAxis("RightStickHorizontal");
        float rv = Input.GetAxis("RightStickVertical");

    }
}