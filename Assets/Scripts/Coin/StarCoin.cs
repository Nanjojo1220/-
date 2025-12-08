using UnityEngine;

public class StarCoin : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CoinManager.instance.AddStarCoin();
            Destroy(gameObject);
        }
    }
}
