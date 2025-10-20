using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float sensitivity = 3.0f;     // ��]���x
    [SerializeField] private float minYAngle = -30f;       // �J�����̉����p�x
    [SerializeField] private float maxYAngle = 60f;        // �J�����̏���p�x
    [SerializeField] private float followSmooth = 10f;     // �Ǐ]�X���[�Y��
    [SerializeField] private float cameraDistance = 6f;    // �v���C���[����̋���

    private float yaw;   // ���E��]
    private float pitch; // �㉺��]

    // Dead Zone �p
    private float deadZone = 0.2f;

    void Start()
    {
        if (player == null)
        {
            UnityEngine.Debug.LogError("CameraController: Player���ݒ肳��Ă��܂���B");
            enabled = false;
            return;
        }

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        // --- �}�E�X���� ---
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // --- �E�X�e�B�b�N���� ---
        float rightX = Input.GetAxis("RightStickHorizontal");
        float rightY = Input.GetAxis("RightStickVertical");

        // Dead Zone �̓K�p
        if (Mathf.Abs(rightX) < deadZone) rightX = 0f;
        if (Mathf.Abs(rightY) < deadZone) rightY = 0f;

        // --- ���͊��x�𔽉f ---
        float inputX = mouseX * sensitivity + rightX * sensitivity;
        float inputY = mouseY * sensitivity + rightY * sensitivity; // �㉺�̕����͕K�v�ɉ����Ĕ��]

        // --- �J������] ---
        yaw += inputX;
        pitch -= inputY; // �㉺��]
        pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // --- �v���C���[����ɔz�u ---
        Vector3 targetPos = player.transform.position - rotation * Vector3.forward * cameraDistance + Vector3.up * 2.0f;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSmooth);
        transform.LookAt(player.transform.position + Vector3.up * 1.5f);
    }

    void Update()
    {
        // �f�o�b�O�p�F�E�X�e�B�b�N�l�m�F
        float rh = Input.GetAxis("RightStickHorizontal");
        float rv = Input.GetAxis("RightStickVertical");
        UnityEngine.Debug.Log($"Right Stick: H={rh:F2}, V={rv:F2}");
    }
}

