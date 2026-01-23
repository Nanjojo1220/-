using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FallRespawn : MonoBehaviour
{
    [Header("残機")]
    [SerializeField] private int maxLives = 3;
    private int currentLives;

    [Header("UI")]
    [SerializeField] private LifeUI lifeUI;

    [Header("リスポーン")]
    [SerializeField] private Transform respawnPoint;

    [Header("演出")]
    [SerializeField] private float shrinkDuration = 0.5f;
    [SerializeField] private float minScale = 0.1f;

    [Header("効果音")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fallSE;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;

    [SerializeField] private AudioClip checkpointBGM; // チェックポイント到達
    [SerializeField] private AudioClip gameOverBGM;   // ゲームオーバー


    private bool isRespawning;
    private bool checkpointPlayed = false;
    private Vector3 originalScale;
    private Animator animator;
    private Rigidbody rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;

        currentLives = maxLives;
    }

    private void Start()
    {
        // ★ UI初期化（超重要）
        if (lifeUI != null)
            lifeUI.UpdateHearts(currentLives);
    }

    private void OnTriggerEnter(Collider other)
    {
        // チェックポイント更新
        if (other.CompareTag("Checkpoint"))
        {
            respawnPoint = other.transform;

            if (!checkpointPlayed && bgmSource && checkpointBGM)
            {
                bgmSource.PlayOneShot(checkpointBGM);
                checkpointPlayed = true;
            }
            return;
        }

        // 落下
        if (other.CompareTag("DeadZone") && !isRespawning)
        {
            currentLives--;

            if (lifeUI != null)
                lifeUI.UpdateHearts(currentLives);

            if (currentLives > 0)
            {
                StartCoroutine(RespawnRoutine());
            }

            else
            {
                StartCoroutine(GameOverSequence());
            }

        }
    }

    private IEnumerator GameOverSequence()
    {
        isRespawning = true;

        if (bgmSource && gameOverBGM)
        {
            bgmSource.Stop();
            bgmSource.clip = gameOverBGM;
            bgmSource.loop = false;
            bgmSource.Play();
        }

        yield return new WaitForSeconds(2.5f);

        SceneManager.LoadScene("GameOver");
    }


    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // 効果音
        if (audioSource && fallSE)
            audioSource.PlayOneShot(fallSE);

        // アニメーション開始
        if (animator)
            animator.SetTrigger("Fall");

        // Rigidbody停止
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        // 縮小演出
        float t = 0f;
        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(
                originalScale,
                originalScale * minScale,
                t / shrinkDuration
            );
            yield return null;
        }

        // リスポーン
        if (respawnPoint != null)
            transform.position = respawnPoint.position;

        transform.localScale = originalScale;

        yield return new WaitForSeconds(0.1f);

        // ★ アニメーション完全リセット（重要）
        if (animator)
        {
            animator.ResetTrigger("Fall");
            animator.Play("Idle", 0, 0f);
        }

        rb.isKinematic = false;
        isRespawning = false;
    }
}
