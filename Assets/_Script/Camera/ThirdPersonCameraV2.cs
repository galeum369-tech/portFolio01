using UnityEngine;

/// <summary>
/// ThirdPersonCameraControllerV2
/// 
/// [역할]
/// - 자유 시 입력 기반 회전
/// - 락온 시 타겟 응시
/// 
/// ⚠ 위치 이동 ❌ (CameraRoot가 담당)
/// </summary>
public class ThirdPersonCameraControllerV2 : MonoBehaviour
{
    [Header("Free Look")]
    [SerializeField] float sensitivity = 0.1f;
    [SerializeField] float minPitch = -30f;
    [SerializeField] float maxPitch = 60f;

    [Header("LockOn")]
    [SerializeField] float lockOnRotateSpeed = 8f;

    [SerializeField] Transform player;

    float yaw;
    float pitch;

    PlayerLockOnV2 lockOn;

    void Awake()
    {
        lockOn = player.GetComponent<PlayerLockOnV2>();
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
        Vector3 dir = lockOn.CurrentTarget.position - player.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * lockOnRotateSpeed
        );
    }
}
