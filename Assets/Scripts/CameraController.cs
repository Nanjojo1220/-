using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float sensitivity = 3.0f;     // 回転感度
    [SerializeField] private float minYAngle = -30f;       // カメラの下限角度
    [SerializeField] private float maxYAngle = 60f;        // カメラの上限角度
    [SerializeField] private float followSmooth = 10f;     // 追従スムーズさ
    [SerializeField] private float cameraDistance = 6f;    // プレイヤーからの距離

    private float yaw;   // 左右回転
    private float pitch; // 上下回転

    // Dead Zone 用
    private float deadZone = 0.2f;

    void Start()
    {
        if (player == null)
        {
            UnityEngine.Debug.LogError("CameraController: Playerが設定されていません。");
            enabled = false;
            return;
        }

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        // --- マウス入力 ---
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // --- 右スティック入力 ---
        float rightX = Input.GetAxis("RightStickHorizontal");
        float rightY = Input.GetAxis("RightStickVertical");

        // Dead Zone の適用
        if (Mathf.Abs(rightX) < deadZone) rightX = 0f;
        if (Mathf.Abs(rightY) < deadZone) rightY = 0f;

        // --- 入力感度を反映 ---
        float inputX = mouseX * sensitivity + rightX * sensitivity;
        float inputY = mouseY * sensitivity + rightY * sensitivity; // 上下の符号は必要に応じて反転

        // --- カメラ回転 ---
        yaw += inputX;
        pitch -= inputY; // 上下回転
        pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // --- プレイヤー後方に配置 ---
        Vector3 targetPos = player.transform.position - rotation * Vector3.forward * cameraDistance + Vector3.up * 2.0f;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSmooth);
        transform.LookAt(player.transform.position + Vector3.up * 1.5f);
    }

    void Update()
    {
        // デバッグ用：右スティック値確認
        float rh = Input.GetAxis("RightStickHorizontal");
        float rv = Input.GetAxis("RightStickVertical");
        UnityEngine.Debug.Log($"Right Stick: H={rh:F2}, V={rv:F2}");
    }
}

