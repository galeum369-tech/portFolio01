using UnityEngine;

/// <summary>
/// CameraRootControllerV2
/// 
/// [역할]
/// - 플레이어 위치 추적
/// - 락온 시 카메라 위치 오프셋 변경
/// - 따라가는 속도 조절
/// </summary>
public class CameraRootControllerV2 : MonoBehaviour
{
    [SerializeField] Transform player;

    [Header("Offset")]
    [SerializeField] Vector3 normalOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] Vector3 lockOnOffset = new Vector3(0.5f, 1.6f, 0f);

    [Header("Follow")]
    [SerializeField] float followSpeed = 10f;

    PlayerLockOnV2 lockOn;

    void Awake()
    {
        lockOn = player.GetComponent<PlayerLockOnV2>();
    }

    void LateUpdate()
    {
        Vector3 offset =
            (lockOn != null && lockOn.IsLockOn)
            ? lockOnOffset
            : normalOffset;

        Vector3 targetPos = player.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * followSpeed
        );
    }
}
