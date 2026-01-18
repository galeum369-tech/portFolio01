using UnityEngine;

/// <summary>
/// PlayerControllerV2 (통합 최종본)
///
/// [State 개념]
/// - 입력 제어권을 관리 (Free / Charging / Attacking / Evading)
///
/// [Mode 개념]
/// - LockOn : 이동/회전/애니메이션 해석 방식만 변경
/// - Guard  : 데미지 경감용 모드 (방향 판정 ❌)
///
/// ⚠ 공격 판정 / 데미지 계산 ❌ (Combat 담당)
/// ⚠ 무적 타이밍 ❌ (Evade Animation Event 담당)
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerCoreV2))]
public class PlayerControllerV2 : MonoBehaviour
{
    /*───────────────────────────────*
     * 상태 정의
     *───────────────────────────────*/
    enum ControllerState
    {
        Free,
        Charging,
        Attacking,
        Evading
    }

    ControllerState state = ControllerState.Free;

    /*───────────────────────────────*
     * 컴포넌트
     *───────────────────────────────*/
    CharacterController cc;
    Animator anim;
    PlayerCoreV2 core;

    [Header("전투")]
    [SerializeField] PlayerCombatV2 combat;

    [Header("카메라")]
    [SerializeField] Transform cameraRoot;

    /*───────────────────────────────*
     * 이동 / 무게감
     *───────────────────────────────*/
    [Header("이동")]
    [SerializeField] float acceleration = 12f;
    [SerializeField] float deceleration = 18f;
    [SerializeField] float sprintMultiplier = 1.5f;

    float currentSpeed;
    Vector3 moveDirection;

    /*───────────────────────────────*
     * 중력
     *───────────────────────────────*/
    float gravity = -20f;
    float verticalVelocity;

    /*───────────────────────────────*
     * 회피
     *───────────────────────────────*/
    [Header("회피")]
    [SerializeField] float rollDistance = 4f;
    [SerializeField] float rollDuration = 0.45f;
    [SerializeField] float backstepDistance = 2.5f;
    [SerializeField] float backstepDuration = 0.3f;

    Vector3 evadeDirection;
    float evadeSpeed;

    /*───────────────────────────────*
     * 차지
     *───────────────────────────────*/
    [Header("차지")]
    [SerializeField] float chargeThreshold = 0.7f;
    float chargeTimer;

    /*───────────────────────────────*
     * 락온 / 가드 (Mode)
     *───────────────────────────────*/
    public bool IsLockOn { get; private set; }
    Transform lockOnTarget;

    bool isGuarding;

    /*───────────────────────────────*
     * Animator Hash
     *───────────────────────────────*/
    int hashMoveX;
    int hashMoveY;
    int hashRoll;
    int hashBackstep;
    int hashBlock;

    /*───────────────────────────────*
     * 초기화
     *───────────────────────────────*/
    void Awake()
    {
        cc = GetComponent<CharacterController>();
        core = GetComponent<PlayerCoreV2>();
        anim = GetComponentInChildren<Animator>();

        if (combat == null)
            combat = GetComponentInChildren<PlayerCombatV2>();

        hashMoveX = Animator.StringToHash("moveX");
        hashMoveY = Animator.StringToHash("moveY");
        hashRoll = Animator.StringToHash("Rolling");
        hashBackstep = Animator.StringToHash("Backstep");
        hashBlock = Animator.StringToHash("Block");
    }

    void OnEnable()
    {
        InputManager.OnAttackStarted += OnAttackStarted;
        InputManager.OnAttackCanceled += OnAttackCanceled;
        InputManager.OnRolling += TryEvade;
    }

    void OnDisable()
    {
        InputManager.OnAttackStarted -= OnAttackStarted;
        InputManager.OnAttackCanceled -= OnAttackCanceled;
        InputManager.OnRolling -= TryEvade;
    }

    /*───────────────────────────────*
     * Update
     *───────────────────────────────*/
    void Update()
    {
        HandleMovement();
        HandleGuard();
        ApplyGravity();

        if (state == ControllerState.Charging)
            chargeTimer += Time.deltaTime;
    }

    /*───────────────────────────────*
     * FixedUpdate (회피 이동)
     *───────────────────────────────*/
    void FixedUpdate()
    {
        if (state == ControllerState.Evading)
        {
            cc.Move(evadeDirection * evadeSpeed * Time.fixedDeltaTime);
        }
    }

    /*───────────────────────────────*
     * 이동 처리
     *───────────────────────────────*/
    void HandleMovement()
    {
        Vector2 input = InputManager.Input;
        bool hasInput = input.sqrMagnitude > 0.01f;

        if (state != ControllerState.Free)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            ApplyMoveAnimation(Vector3.zero);
            return;
        }

        Vector3 camForward = Vector3.ProjectOnPlane(cameraRoot.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraRoot.right, Vector3.up).normalized;
        moveDirection = (camForward * input.y + camRight * input.x).normalized;

        float targetSpeed = hasInput ? core.WalkSpeed : 0f;

        if (hasInput && InputManager.IsSprint && !isGuarding)
        {
            if (core.TryConsumeRunStamina(Time.deltaTime))
                targetSpeed *= sprintMultiplier;
        }

        float accel = targetSpeed > currentSpeed ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accel * Time.deltaTime);

        if (currentSpeed > 0.01f)
        {
            cc.Move(moveDirection * currentSpeed * Time.deltaTime);
            HandleRotation(hasInput);
        }

        ApplyMoveAnimation(moveDirection);
    }

    void HandleRotation(bool hasInput)
    {
        if (IsLockOn && lockOnTarget != null)
        {
            Vector3 dir = lockOnTarget.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 12f);
            }
        }
        else if (hasInput && moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
        }
    }

    void ApplyMoveAnimation(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.001f)
        {
            anim.SetFloat(hashMoveX, 0f);
            anim.SetFloat(hashMoveY, 0f);
            return;
        }

        if (!IsLockOn)
        {
            anim.SetFloat(hashMoveX, 0f);
            anim.SetFloat(hashMoveY, currentSpeed / core.WalkSpeed);
        }
        else
        {
            Vector3 local = transform.InverseTransformDirection(dir);
            anim.SetFloat(hashMoveX, local.x);
            anim.SetFloat(hashMoveY, local.z);
        }
    }

    /*───────────────────────────────*
     * 중력
     *───────────────────────────────*/
    void ApplyGravity()
    {
        if (cc.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    /*───────────────────────────────*
     * 공격 / 차지
     *───────────────────────────────*/
    void OnAttackStarted()
    {
        if (state != ControllerState.Free || combat.IsInAction)
            return;

        state = ControllerState.Charging;
        chargeTimer = 0f;
    }

    void OnAttackCanceled()
    {
        if (state != ControllerState.Charging)
            return;

        if (chargeTimer >= chargeThreshold)
            combat.ExecuteChargeAttack();
        else
            combat.ExecuteNormalAttack();

        state = ControllerState.Attacking;
        Invoke(nameof(ReleaseAttackState), 0.1f);
    }

    void ReleaseAttackState()
    {
        state = ControllerState.Free;
    }

    /*───────────────────────────────*
     * 회피 (입력 → 애니메이션만)
     *───────────────────────────────*/
    void TryEvade()
    {
        if (state != ControllerState.Free)
            return;

        if (!core.TryConsumeRollStamina())
            return;

        bool hasInput = InputManager.Input.magnitude > 0.1f;
        evadeDirection = hasInput ? moveDirection : -transform.forward;

        float distance = hasInput ? rollDistance : backstepDistance;
        float duration = hasInput ? rollDuration : backstepDuration;
        evadeSpeed = distance / duration;

        state = ControllerState.Evading;
        anim.SetTrigger(hasInput ? hashRoll : hashBackstep);
    }

    /*───────────────────────────────*
     * Evade Animation Event 수신
     *───────────────────────────────*/
    public void OnEvadeStart()
    {
        // 이동은 FixedUpdate에서 처리
    }

    public void OnEvadeEnd()
    {
        state = ControllerState.Free;
    }

    public void OnInvincibleStart()
    {
        core.SetInvincible(true);
    }

    public void OnInvincibleEnd()
    {
        core.SetInvincible(false);
    }

    /*───────────────────────────────*
     * 가드
     *───────────────────────────────*/
    void HandleGuard()
    {
        bool guard = InputManager.IsBlock;
        core.SetGuard(guard);
        anim.SetBool(hashBlock, guard);
    }


    /*───────────────────────────────*
     * 락온 연동
     *───────────────────────────────*/
    public void SetLockOn(bool value, Transform target)
    {
        IsLockOn = value;
        lockOnTarget = target;
    }
}







