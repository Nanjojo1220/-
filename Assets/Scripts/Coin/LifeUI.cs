using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class LifeUI : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image[] hearts;

    public void UpdateHearts(int currentLives)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < currentLives;
        }
    }
}


