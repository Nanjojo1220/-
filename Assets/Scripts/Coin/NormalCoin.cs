using UnityEngine;

public class NormalCoin : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CoinManager.instance.AddNormalCoin();
            Destroy(gameObject);
        }
    }
}
