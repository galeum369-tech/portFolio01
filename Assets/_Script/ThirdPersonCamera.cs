using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [SerializeField] float sensitivity = 0.1f;
    [SerializeField] float minPitch = -30f;
    [SerializeField] float maxPitch = 60f;

    float yaw;
    float pitch;

    void LateUpdate()
    {
        Vector2 look = InputManager.Look; // ⭐ 여기 중요

        yaw += look.x * sensitivity;
        pitch -= look.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
