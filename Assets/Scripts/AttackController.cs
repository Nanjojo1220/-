using System.Diagnostics;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 30f;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton7)) // R2
        {
            Shoot();
        }
    }


    void Shoot()
    {
        // ‡@ ƒJƒƒ‰’†‰›‚©‚ç Ray
        Ray camRay = mainCamera.ViewportPointToRay(
    new Vector3(0.5f, 0.5f, 0f)
);


        Vector3 targetPoint;

        if (Physics.Raycast(camRay, out RaycastHit camHit, range, hitMask))
        {
            targetPoint = camHit.point;   // ƒJƒƒ‰‚ªŒ©‚Ä‚¢‚éêŠ
        }
        else
        {
            targetPoint = camRay.GetPoint(range);
        }

        // ‡A eŒûi‚Ü‚½‚ÍƒLƒƒƒ‰j‚©‚ç targetPoint ‚ÖŒü‚¯‚é
        Vector3 shootDir = (targetPoint - shootPoint.position).normalized;

        GameObject bullet = Instantiate(
        bulletPrefab,
        shootPoint.position + shootDir * 0.2f,   // © ­‚µ‘O
        Quaternion.LookRotation(shootDir)
    );

        // ‡C ’e‚É•ûŒü‚ğ“n‚·
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.Init(shootDir);

        // ‡D ³Šm‚È“–‚½‚è”»’èiRayj
        if (Physics.Raycast(shootPoint.position, shootDir, out RaycastHit hit, range, hitMask))
        {
            UnityEngine.Debug.Log("Hit : " + hit.collider.name);
        }
    }
       
    }
