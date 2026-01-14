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

    LockOnSystem lockOn;

    void Awake()
    {
        lockOn = player.GetComponent<LockOnSystem>();
    }

    void LateUpdate()
    {
        if (lockOn != null && lockOn.IsLockOn && lockOn.lockOnTarget != null)
        {
            LockOnLook();
        }
        else
        {
            FreeLook();
        }

        transform.position = player.position + offset;
    }

    void FreeLook()
    {
        Vector2 look = InputManager.Look;

        yaw += look.x * sensitivity;
        pitch -= look.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void LockOnLook()
    {
        Vector3 dir = lockOn.lockOnTarget.position - transform.position;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

        // 부드럽게 고정
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * 10f
        );
    }
}


