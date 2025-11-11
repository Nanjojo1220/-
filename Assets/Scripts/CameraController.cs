using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

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
        pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);


        Vector3 targetCenter = player.transform.position + Vector3.up * verticalOffset;
        Vector3 desiredPos = targetCenter - rotation * Vector3.forward * cameraDistance;


        RaycastHit hit;
        if (Physics.SphereCast(targetCenter, cameraRadius, rotation * Vector3.back, out hit, cameraDistance, collisionMask))
        {
            // 障害物にぶつかる前にカメラを手前に寄せる
            desiredPos = hit.point + hit.normal * cameraRadius;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followSmooth);
        transform.LookAt(player.transform.position + Vector3.up * lookOffsetY);

    }

    // Update is called once per frame
    void Update()
    {

        float rh = Input.GetAxis("RightStickHorizontal");
        float rv = Input.GetAxis("RightStickVertical");
        UnityEngine.Debug.Log($"Right Stick: H={rh:F2}, V={rv:F2}");

    }
}
