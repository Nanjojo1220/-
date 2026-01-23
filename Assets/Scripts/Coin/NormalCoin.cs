using UnityEngine;

public class NormalCoin : MonoBehaviour
{
    [Header("å¯â âπ")]
    [SerializeField] private AudioClip getSE;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Åö SEçƒê∂
            if (getSE != null)
            {
                AudioSource.PlayClipAtPoint(
                    getSE,
                    transform.position
                );
            }

            CoinManager.instance.AddNormalCoin();
            Destroy(gameObject);
        }
    }
}
