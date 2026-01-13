using UnityEngine;

public class movewall : MonoBehaviour
{
    [Header("動き")]
    [SerializeField] public float moveDistance = 3f;
    [SerializeField] public float moveSpeed = 2f;
    [SerializeField] public float waitTime = 1f;
    [SerializeField] public float startDelay = 1f;

    private Vector3 startPos;
    private bool movingForward = true;
    private float waitTimer = 0f;
    private Rigidbody rb;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // ★ 見た目安定

        waitTimer = startDelay;
    }

    void FixedUpdate()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 targetPos =
            startPos + new Vector3(0, 0, movingForward ? moveDistance : -moveDistance);

        // ★ 次の位置を先に計算
        Vector3 nextPos = Vector3.MoveTowards(
            rb.position,
            targetPos,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPos);

        // ★ 到達判定（誤差なし）
        if (nextPos == targetPos)
        {
            movingForward = !movingForward;
            waitTimer = waitTime;
        }
    }
}
