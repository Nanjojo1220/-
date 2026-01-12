using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;

    [Header("Rotation Limit")]
    [SerializeField] private float normalMinY = -30f;
    [SerializeField] private float normalMaxY = 60f;
    [SerializeField] private float aimMinY = -80f;
    [SerializeField] private float aimMaxY = 80f;

    [Header("Follow")]
    [SerializeField] private float followSmooth = 10f;
    [SerializeField] private float cameraDistance = 6f;
    [SerializeField] private float aimDistance = 4.5f;

    [Header("Look Offset")]
    [SerializeField] private float normalLookOffsetY = 1.5f;
    [SerializeField] private float aimLookOffsetY = 1.8f;

    [Header("Shoulder Offset (BOTW)")]
    [SerializeField] private float normalShoulderOffsetX = 0.3f;
    [SerializeField] private float aimShoulderOffsetX = 0.6f;

    [Header("Sensitivity")]
    [SerializeField] private float normalSensitivity = 3.0f;
    [SerializeField] private float aimSensitivity = 1.2f;

    [Header("FOV")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float aimFOV = 40f;
    [SerializeField] private float fovSmooth = 10f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float cameraRadius = 0.3f;

    [Header("Rotate Smooth")]
    [SerializeField] private float rotateSmoothTime = 0.05f;

    [Header("Reticle")]
    public GameObject reticle;

    private float yaw;
    private float pitch;
    private float smoothYaw;
    private float smoothPitch;
    private float yawVelocity;
    private float pitchVelocity;

    private Vector3 camVelocity;
    private float deadZone = 0.2f;

    private Renderer lastObstacleRenderer;
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

    void Start()
    {
        if (player == null)
        {
            UnityEngine.Debug.LogError("CameraController: Playerが未設定です");
            enabled = false;
            return;
        }

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        bool isAiming = Input.GetKey(KeyCode.JoystickButton6); // L2

        // --- 入力 ---
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float rightX = Input.GetAxis("RightStickHorizontal");
        float rightY = Input.GetAxis("RightStickVertical");

        if (Mathf.Abs(rightX) < deadZone) rightX = 0f;
        if (Mathf.Abs(rightY) < deadZone) rightY = 0f;

        float sensitivity = isAiming ? aimSensitivity : normalSensitivity;

        yaw += (mouseX + rightX) * sensitivity;
        pitch -= (mouseY + rightY) * sensitivity;

        float minPitch = isAiming ? aimMinY : normalMinY;
        float maxPitch = isAiming ? aimMaxY : normalMaxY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        smoothYaw = Mathf.SmoothDampAngle(smoothYaw, yaw, ref yawVelocity, rotateSmoothTime);
        smoothPitch = Mathf.SmoothDampAngle(smoothPitch, pitch, ref pitchVelocity, rotateSmoothTime);

        Quaternion rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0);

        // --- 注視点 ---
        float lookY = isAiming ? aimLookOffsetY : normalLookOffsetY;
        Vector3 targetCenter = player.transform.position + Vector3.up * lookY;

        // --- BOTW右肩パラメータ ---
        float shoulderX = isAiming ? aimShoulderOffsetX : normalShoulderOffsetX;
        float distance = isAiming ? aimDistance : cameraDistance;

        // --- 理想位置 ---
        Vector3 desiredPos =
            targetCenter
            - rotation * Vector3.forward * distance
            + rotation * Vector3.right * shoulderX;

        // --- カメラ衝突処理 ---
        Vector3 dir = (desiredPos - targetCenter).normalized;
        float currentDistance = distance;

        if (Physics.SphereCast(
            targetCenter,
            cameraRadius,
            dir,
            out RaycastHit hit,
            distance,
            collisionMask
        ))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Object"))
            {
                Renderer r = hit.collider.GetComponent<Renderer>();
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
                currentDistance = Mathf.Max(hit.distance - cameraRadius, 0.5f);

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
            if (lastObstacleRenderer != null &&
                originalColors.ContainsKey(lastObstacleRenderer))
            {
                lastObstacleRenderer.material.color =
                    originalColors[lastObstacleRenderer];
                lastObstacleRenderer = null;
            }
        }

        // --- 最終位置（右肩維持） ---
        Vector3 finalPos =
            targetCenter
            - rotation * Vector3.forward * currentDistance
            + rotation * Vector3.right * shoulderX;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            finalPos,
            ref camVelocity,
            0.2f
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotation,
            Time.deltaTime * followSmooth
        );

        // --- FOV ---
        Camera cam = GetComponent<Camera>();
        float targetFOV = isAiming ? aimFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovSmooth);
    }

    void Update()
    {
        bool isAiming = Input.GetKey(KeyCode.JoystickButton6);
        if (reticle != null)
            reticle.SetActive(isAiming);
    }
}
