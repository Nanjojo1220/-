using UnityEngine;
using TMPro;

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

    private void Start()
    {
        UpdateUI();
    }

    public void AddStarCoin()
    {
        starCoinCount++;
        UpdateUI();

        if (IsGoal())
        {
            UnityEngine.Debug.Log("スターコインを全て取得！クリア可能！");
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
}
