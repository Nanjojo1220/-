using UnityEngine;

public class MoveFloor : MonoBehaviour
{
    public enum MoveDirection
    {
        X,
        Y,
        Z
    }

    [Header("“®‚«")]
    [SerializeField] private MoveDirection direction = MoveDirection.Z;
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private float startDelay = 0f;

    private Vector3 startPos;
    private bool movingForward = true;
    private float waitTimer = 0f;
    private Rigidbody rb;

    private bool isActive = false;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        waitTimer = startDelay;
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 dir = GetDirectionVector();
        Vector3 targetPos = startPos + dir * (movingForward ? moveDistance : -moveDistance);

        Vector3 nextPos = Vector3.MoveTowards(
            rb.position,
            targetPos,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPos);

        if (nextPos == targetPos)
        {
            movingForward = !movingForward;
            waitTimer = waitTime;
        }
    }

    Vector3 GetDirectionVector()
    {
        switch (direction)
        {
            case MoveDirection.X: return Vector3.right;
            case MoveDirection.Y: return Vector3.up;
            case MoveDirection.Z: return Vector3.forward;
            default: return Vector3.forward;
        }
    }

    public void Activate()
    {
        isActive = true;
    }
}
