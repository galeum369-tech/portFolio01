using UnityEngine;
using UnityEngine.InputSystem;

public class LockOnSystem : MonoBehaviour
{
    public Transform lockOnTarget;   // Dummy/Target
    public bool IsLockOn { get; private set; }

    PlayerController player;

    void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        // 🔹 테스트용 락온 토글 (휠 클릭 / 임시로 L 키)
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            ToggleLockOn();
        }

        // 🔹 락온 중이면 타겟 바라보기
        if (IsLockOn && lockOnTarget != null)
        {
            Vector3 dir = lockOnTarget.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    rot,
                    Time.deltaTime * 10f
                );
            }
        }
    }

    void ToggleLockOn()
    {
        IsLockOn = !IsLockOn;
        player.IsLockOn = IsLockOn;

        Debug.Log(IsLockOn ? "LockOn ON" : "LockOn OFF");
    }
}

