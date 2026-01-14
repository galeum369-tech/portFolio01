using UnityEngine;
using System.Collections;

/// <summary>
/// PlayerControllerV2
/// 
/// [역할]
/// - 이동 / 회전 / 중력 처리
/// - 회피 실행(구르기/백스텝)
/// - 차지 입력 판단(시간 측정)
/// - 컨트롤 락(이동/회전 제한) 관리
/// - 락온 여부에 따른 이동 애니메이션 보정(moveX / moveY)
/// - 락온 중 캐릭터 회전 고정(타겟 응시)
/// 
/// ⚠ 공격 판정 / 데미지 계산 / 투사체 생성 ❌ (Combat 담당)
/// ⚠ 애니메이션 상태 해석 ❌ (Combat 담당)
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerCoreV2))]
public class PlayerControllerV2 : MonoBehaviour
{
    /*───────────────────────────────*
     * 컴포넌트
     *───────────────────────────────*/
    CharacterController cc;
    Animator anim;
    PlayerCoreV2 core;

    [Header("전투")]
    [SerializeField] PlayerCombatV2 combat;

    /*───────────────────────────────*
     * 카메라
     *───────────────────────────────*/
    [Header("카메라")]
    [SerializeField] Transform cameraRoot;

    /*───────────────────────────────*
     * 이동 설정 (무게감)
     *───────────────────────────────*/
    [Header("이동(무게감)")]
    [SerializeField] float acceleration = 12f;
    [SerializeField] float deceleration = 18f;

    float currentSpeed;
    Vector3 moveDirection;

    /*───────────────────────────────*
     * 중력
     *───────────────────────────────*/
    float gravity = -20f;
    float verticalVelocity;
    bool isGrounded;

    /*───────────────────────────────*
     * 회피
     *───────────────────────────────*/
    [Header("회피")]
    [SerializeField] float rollDistance = 4f;
    [SerializeField] float backstepDistance = 2f;

    bool isEvading;
    Vector3 evadeDirection;

    /*───────────────────────────────*
     * 차지
     *───────────────────────────────*/
    [Header("차지")]
    [Tooltip("이 시간 이상 누르면 차지 공격으로 판정")]
    [SerializeField] float chargeThreshold = 0.7f;

    bool isCharging;
    float chargeTimer;

    /*───────────────────────────────*
     * 락온
     *───────────────────────────────*/
    [Header("락온")]
    public bool IsLockOn { get; private set; }
    Transform lockOnTarget;

    /*───────────────────────────────*
     * 상태 플래그
     *───────────────────────────────*/
    bool isInAction;
    bool canMove = true;
    bool canRotate = true;

    /*───────────────────────────────*
     * Animator Hash
     *───────────────────────────────*/
    int hashMoveX;
    int hashMoveY;
    int hashRoll;
    int hashBackstep;

    /*───────────────────────────────*
     * 초기화
     *───────────────────────────────*/
    void Awake()
    {
        cc = GetComponent<CharacterController>();
        core = GetComponent<PlayerCoreV2>();
        anim = GetComponentInChildren<Animator>();

        hashMoveX = Animator.StringToHash("moveX");
        hashMoveY = Animator.StringToHash("moveY");
        hashRoll = Animator.StringToHash("Rolling");
        hashBackstep = Animator.StringToHash("Backstep");

        if (combat == null)
            combat = GetComponentInChildren<PlayerCombatV2>();

        if (cameraRoot == null)
            Debug.LogWarning("[ControllerV2] cameraRoot가 비어있습니다.");
    }

    void OnEnable()
    {
        InputManager.OnAttackStarted += OnAttackStarted;
        InputManager.OnAttackCanceled += OnAttackCanceled;
    }

    void OnDisable()
    {
        InputManager.OnAttackStarted -= OnAttackStarted;
        InputManager.OnAttackCanceled -= OnAttackCanceled;
    }

    void Update()
    {
        HandleMovement();
        ApplyGravity();

        if (isCharging)
            chargeTimer += Time.deltaTime;
    }

    /*───────────────────────────────*
     * 이동 처리
     *───────────────────────────────*/
    void HandleMovement()
    {
        if (!canMove || cameraRoot == null)
        {
            anim.SetFloat(hashMoveX, 0f);
            anim.SetFloat(hashMoveY, 0f);
            return;
        }

        Vector2 input = InputManager.Input;

        // 카메라 기준 이동 방향
        Vector3 camForward = cameraRoot.forward;
        Vector3 camRight = cameraRoot.right;
        camForward.y = 0f;
        camRight.y = 0f;

        moveDirection = (camForward * input.y + camRight * input.x).normalized;

        // 무게감 있는 속도 변화
        float targetSpeed = input.magnitude > 0.1f ? core.WalkSpeed : 0f;
        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            (targetSpeed > currentSpeed ? acceleration : deceleration) * Time.deltaTime
        );

        cc.Move(moveDirection * currentSpeed * Time.deltaTime);

        /*──────── 회전 분기 ────────*/
        if (canRotate)
        {
            // 🔒 락온 중: 항상 타겟을 바라봄
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
                        Time.deltaTime * 12f
                    );
                }
            }
            // 자유 상태: 이동 방향으로 회전
            else if (moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    rot,
                    Time.deltaTime * 10f
                );
            }
        }

        ApplyMoveAnimation(input);
    }

    /*───────────────────────────────*
     * 이동 애니메이션 보정
     *───────────────────────────────*/
    void ApplyMoveAnimation(Vector2 input)
    {
        if (input.magnitude < 0.1f)
        {
            anim.SetFloat(hashMoveX, 0f);
            anim.SetFloat(hashMoveY, 0f);
            return;
        }

        if (!IsLockOn)
        {
            // 비 락온: 항상 정면 전진 느낌
            anim.SetFloat(hashMoveX, 0f);
            anim.SetFloat(hashMoveY, 1f);
        }
        else
        {
            // 락온: 사이드워크 / 후진 허용
            Vector3 localDir = transform.InverseTransformDirection(moveDirection);
            anim.SetFloat(hashMoveX, localDir.x);
            anim.SetFloat(hashMoveY, localDir.z);
        }
    }

    /*───────────────────────────────*
     * 중력
     *───────────────────────────────*/
    void ApplyGravity()
    {
        isGrounded = cc.isGrounded;

        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    /*───────────────────────────────*
     * 공격 입력 (차지 판단)
     *───────────────────────────────*/
    void OnAttackStarted()
    {
        if (isInAction || combat == null || combat.IsInAction)
            return;

        isCharging = true;
        chargeTimer = 0f;

        isInAction = true;
        canMove = false;
        canRotate = false;

        Debug.Log("[ControllerV2] 공격 입력 시작 (차지 측정)");
    }

    void OnAttackCanceled()
    {
        if (!isCharging)
            return;

        isCharging = false;

        Debug.Log($"[ControllerV2] 공격 입력 종료 (차지 {chargeTimer:F2}s)");

        if (chargeTimer >= chargeThreshold)
            combat.ExecuteChargeAttack();
        else
            combat.ExecuteNormalAttack();

        ReleaseControlLock();
    }

    void ReleaseControlLock()
    {
        isInAction = false;
        canMove = true;
        canRotate = true;
    }

    /*───────────────────────────────*
     * 회피
     *───────────────────────────────*/
    public void TryEvade()
    {
        if (isInAction)
            return;

        if (!core.TryConsumeRollStamina())
        {
            Debug.Log("[ControllerV2] 회피 실패: 스태미나 부족");
            return;
        }

        bool hasInput = InputManager.Input.magnitude > 0.1f;
        evadeDirection = hasInput ? moveDirection : -transform.forward;

        isInAction = true;
        isEvading = true;
        canMove = false;
        canRotate = false;

        Debug.Log(hasInput ? "[ControllerV2] 구르기" : "[ControllerV2] 백스텝");

        if (hasInput) anim.SetTrigger(hashRoll);
        else anim.SetTrigger(hashBackstep);
    }

    public void OnEvadeStart()
    {
        StartCoroutine(EvadeMoveCoroutine());
    }

    public void OnEvadeEnd()
    {
        isEvading = false;
        isInAction = false;
        canMove = true;
        canRotate = true;
    }

    public void OnInvincibleStart()
    {
        core.SetInvincible(true);
    }

    public void OnInvincibleEnd()
    {
        core.SetInvincible(false);
    }

    IEnumerator EvadeMoveCoroutine()
    {
        float distance = evadeDirection == -transform.forward
            ? backstepDistance
            : rollDistance;

        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        if (animLength <= 0.01f) animLength = 0.4f;

        float speed = distance / animLength;
        float elapsed = 0f;

        while (elapsed < animLength)
        {
            cc.Move(evadeDirection * speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    /*───────────────────────────────*
     * 락온 시스템에서 호출
     *───────────────────────────────*/
    public void SetLockOn(bool value, Transform target)
    {
        IsLockOn = value;
        lockOnTarget = target;

        Debug.Log(value ? "[ControllerV2] 락온 ON" : "[ControllerV2] 락온 OFF");
    }

    public Transform GetLockOnTarget()
    {
        return lockOnTarget;
    }

    /*───────────────────────────────*
     * Gizmo
     *───────────────────────────────*/
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + moveDirection * 2f);

        if (isEvading)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + evadeDirection * 3f);
        }
    }
}





