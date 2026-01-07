using UnityEngine;
using UnityEngine.InputSystem;

public class GamePlayFocusController : MonoBehaviour
{
    void Start()
    {
        EnterPlayMode();
    }

    void Update()
    {
        // ESC 키로 플레이 해제
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ExitPlayMode();
        }
    }

    void EnterPlayMode()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
    }

    void ExitPlayMode()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDisable()
    {
        ExitPlayMode();
    }
}

