using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 30f;
    public float lifeTime = 3f;

    private Vector3 direction;

    public void Init(Vector3 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
