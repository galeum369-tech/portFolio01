using UnityEngine;

public class ThirdPersonCameraControllerV2 : MonoBehaviour
{
    [Header("Free Look")]
    [SerializeField] float sensitivity = 0.1f;
    [SerializeField] float minPitch = -30f;
    [SerializeField] float maxPitch = 60f;

    [Header("LockOn")]
    [SerializeField] float lockOnRotateSpeed = 8f;

    [Header("References")]
    [SerializeField] PlayerLockOnV2 lockOn;   // 🔥 직접 연결

    float yaw;
    float pitch;

    void Awake()
    {
        if (lockOn == null)
            Debug.LogWarning("[CameraV2] PlayerLockOnV2가 연결되지 않았습니다.");
    }

    void LateUpdate()
    {
        if (lockOn != null && lockOn.IsLockOn && lockOn.CurrentTarget != null)
            LockOnLook();
        else
            FreeLook();
    }

    void FreeLook()
    {
        Vector2 look = InputManager.Look;

        yaw += look.x * sensitivity;
        pitch -= look.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void LockOnLook()
    {
        Vector3 dir = lockOn.CurrentTarget.position - lockOn.transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRot,
            Time.deltaTime * lockOnRotateSpeed
        );
    }
}
