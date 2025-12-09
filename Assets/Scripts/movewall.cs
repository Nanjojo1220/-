using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movewall : MonoBehaviour
{
    [Header("動き")]
    [SerializeField] public float moveDistance = 3f; // Z方向に動く距離
    [SerializeField] public float moveSpeed = 2f;    // 移動速度
    [SerializeField] public float waitTime = 1f;     // 端で止まる時間
    [SerializeField] public float startDelay = 1f;   // ★ 初動の待機時間（追加）

    private Vector3 startPos;
    private bool movingForward = true;
    private float waitTimer = 0f;
    private Rigidbody rb;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; 

        waitTimer = startDelay; 
    }

    void FixedUpdate()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 targetPos = startPos + new Vector3(0, 0, movingForward ? moveDistance : -moveDistance);

        // MovePosition で滑らかに動かす
        rb.MovePosition(Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.fixedDeltaTime));

        // 端に到達したら反転＋待機
        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            movingForward = !movingForward;
            waitTimer = waitTime;
        }
    }
}
