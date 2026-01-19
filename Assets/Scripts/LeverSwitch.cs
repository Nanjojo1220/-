using UnityEngine;

public class LeverSwitch : MonoBehaviour
{
    [SerializeField] private MoveFloor targetWall;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip onSE;

    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag("Bullet"))
        {
            activated = true;

            // Åö å¯â âπ
            if (audioSource != null && onSE != null)
            {
                audioSource.PlayOneShot(onSE);
            }

            // Åö ìÆÇ≠è∞ÇãNìÆ
            if (targetWall != null)
            {
                targetWall.Activate();
            }
        }
    }
}
