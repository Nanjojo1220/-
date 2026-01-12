using System.Diagnostics;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Camera mainCamera;

    [Header("Shoot")]
    [SerializeField] private float range = 100f;
    [SerializeField] private float minAimDistance = 1.5f;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 30f;

    void Update()
    {
        bool isAiming = Input.GetKey(KeyCode.JoystickButton6); // L2

        // Åö AIMíÜÇÃÇ›î≠éÀâ¬î\
        if (isAiming && Input.GetKeyDown(KeyCode.JoystickButton7)) // R2
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (shootPoint == null || mainCamera == null) return;

        // --- á@ ÉJÉÅÉâíÜâõÇ©ÇÁ Ray ---
        Ray camRay = mainCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 aimPoint;

        if (Physics.Raycast(camRay, out RaycastHit camHit, range, hitMask))
        {
            if (camHit.distance < minAimDistance)
            {
                aimPoint = camRay.GetPoint(range);
            }
            else
            {
                aimPoint = camHit.point;
            }
        }
        else
        {
            aimPoint = camRay.GetPoint(range);
        }

        // --- áA èeå˚ Å® è∆èÄì_ ---
        Vector3 shootDir = (aimPoint - shootPoint.position).normalized;

        // --- áB íeê∂ê¨ ---
        GameObject bullet = Instantiate(
            bulletPrefab,
            shootPoint.position + shootDir * 0.2f,
            Quaternion.LookRotation(shootDir)
        );

        // --- áC íeÇ…ï˚å¸ÇìnÇ∑ ---
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Init(shootDir);
        }

        // --- áD ê≥ämÇ»RayîªíË ---
        if (Physics.Raycast(shootPoint.position, shootDir, out RaycastHit hit, range, hitMask))
        {
            UnityEngine.Debug.Log("Hit : " + hit.collider.name);
        }
    }
}
