using System.Diagnostics;
using UnityEngine;

public class ControllerCheck : MonoBehaviour
{
    void Update()
    {
        string[] joysticks = Input.GetJoystickNames();
        if (joysticks.Length == 0)
        {
            UnityEngine.Debug.Log(" �R���g���[���[���ڑ�");
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

