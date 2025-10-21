using System.Diagnostics;
using UnityEngine;

public class ControllerCheck : MonoBehaviour
{
    void Update()
    {
        string[] joysticks = Input.GetJoystickNames();
        if (joysticks.Length == 0)
        {
            UnityEngine.Debug.Log(" コントローラー未接続");
        }
        else
        {
            for (int i = 0; i < joysticks.Length; i++)
            {
                UnityEngine.Debug.Log($" {i}: {joysticks[i]}");
            }
        }
    }
}

