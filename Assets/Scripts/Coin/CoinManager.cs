using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.Collections;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;

    private void Awake()
    {
        instance = this;
    }

    [Header("UI 表示")]
    public TextMeshProUGUI starCoinText;
    public TextMeshProUGUI normalCoinText;

    [Header("コイン数")]
    public int starCoinCount = 0;
    public int normalCoinCount = 0;

    [Header("スターコイン必要数")]
    public int starCoinGoal = 5; // Inspector で変更可

    [Header("クリア演出")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip clearBGM;
    [SerializeField] private float clearWaitTime = 2f;
    [SerializeField] private string clearSceneName = "GameClear";

    [Header("プレイヤー制御")]
    [SerializeField] private MonoBehaviour playerController;


    private bool isCleared = false;

    private void Start()
    {
        UpdateUI();
    }

    public void AddStarCoin()
    {
        starCoinCount++;
        UpdateUI();

        if(IsGoal() && !isCleared)
    {
            isCleared = true;
            StartCoroutine(ClearSequence());
        }
    }

    public void AddNormalCoin()
    {
        normalCoinCount++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (starCoinText != null)
            starCoinText.text = $"{starCoinCount}/{starCoinGoal}";

        if (normalCoinText != null)
            normalCoinText.text = normalCoinCount.ToString();
    }

    public bool IsGoal()
    {
        return starCoinCount >= starCoinGoal;
    }

    IEnumerator ClearSequence()
    {
        // ★ 操作停止
        if (playerController != null)
            playerController.enabled = false;

        // BGM 再生など
        if (bgmSource != null && clearBGM != null)
        {
            bgmSource.Stop();
            bgmSource.clip = clearBGM;
            bgmSource.Play();
        }

        yield return new WaitForSeconds(clearWaitTime);

        SceneManager.LoadScene(clearSceneName);
    }


}
