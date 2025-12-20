using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.up;
    public float moveDistance = 3f;
    public float moveSpeed = 2f;

    private Vector3 startPos;
    private Rigidbody rb;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float t = Mathf.PingPong(Time.time * moveSpeed, 1);
        Vector3 targetPos = startPos + moveDirection.normalized * moveDistance * t;
        rb.MovePosition(targetPos);
    }
}
