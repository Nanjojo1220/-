using UnityEngine;

public class MoveFloor : MonoBehaviour
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

    // ★ 追加：起動フラグ
    private bool isActive = false;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        waitTimer = startDelay;
    }

    void FixedUpdate()
    {
        // ★ 起動するまで何もしない
        if (!isActive) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 targetPos =
            startPos + new Vector3(0, 0, movingForward ? moveDistance : -moveDistance);

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

    // ★ 外部から呼ぶ起動用メソッド
    public void Activate()
    {
        isActive = true;
    }
}
