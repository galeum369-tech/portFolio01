using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [SerializeField] float sensitivity = 0.1f;
    [SerializeField] float minPitch = -30f;
    [SerializeField] float maxPitch = 60f;

    [SerializeField] Transform player;
    [SerializeField] Vector3 offset = new Vector3(0f, 1.6f, 0f);


    float yaw;
    float pitch;

    void LateUpdate()
    {
        Vector2 look = InputManager.Look;

        yaw += look.x * sensitivity;
        pitch -= look.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = player.position + offset;
    }
}

