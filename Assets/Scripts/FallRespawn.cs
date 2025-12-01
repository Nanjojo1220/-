using System.Collections;
using UnityEngine;

public class FallRespawn : MonoBehaviour
{
    [Header("設定")]
    public Transform respawnPoint;
    public float respawnDelay = 1f;

    [Header("演出")]
    public float shrinkDuration = 0.5f;
    public float minScale = 0.1f;

    [Header("効果音")]
    public AudioSource audioSource;     // ★ 効果音用
    public AudioClip fallSE;            // ★ 落下時の効果音

    private bool isRespawning = false;
    private Vector3 originalScale;

    private Animator animator;

    private void Start()
    {
        originalScale = transform.localScale;
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeadZone") && !isRespawning)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // ▼ 効果音再生
        if (audioSource != null && fallSE != null)
        {
            audioSource.PlayOneShot(fallSE);
        }

        // ▼ 落下アニメ再生
        if (animator != null)
            animator.SetTrigger("Fall");

        // ▼ Rigidbody 停止
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // ▼ 縮小演出
        float t = 0;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = originalScale * minScale;

        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, t / shrinkDuration);
            yield return null;
        }

        // ▼ リスポーン位置にワープ
        transform.position = respawnPoint.position;

        // ▼ サイズを復元
        transform.localScale = originalScale;

        // ▼ 待機
        yield return new WaitForSeconds(respawnDelay);

        // ▼ Idle に戻す
        if (animator != null)
        {
            animator.ResetTrigger("Fall");
            animator.Play("Idle");   // Idleへ強制遷移
        }

        // ▼ Rigidbody復活
        if (rb != null)
            rb.isKinematic = false;

        isRespawning = false;
    }
}
