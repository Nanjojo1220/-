using UnityEngine;

public class StarCoin : MonoBehaviour
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

            CoinManager.instance.AddStarCoin();
            Destroy(gameObject);
        }
    }
}

