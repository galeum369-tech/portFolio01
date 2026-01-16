using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 플레이어 이동 및 입력 처리 컨트롤러
/// - 이동 / 회전 / 중력 처리
/// - PlayerCore 기반 스태미나 연동
/// - 공격 / 가드 / 구르기 제어
/// - 입력 기반 구르기 + 입력 없음 시 백스텝
/// </summary>
public class PlayerController : MonoBehaviour
{
    Animator anim;
    CharacterController cc;

    PlayerCombat pCombat;
    PlayerCore core;

    [Header("카메라")]
    public Transform cameraRoot;

    [Header("전투 상태")]
    public bool IsLockOn = false;

    /*───────────────────────────────*
     * 중력 처리
     *───────────────────────────────*/
    float gravity = -20f;
    float verticalVelocity;
    bool isGrounded;

    /*───────────────────────────────*
     * 구르기 설정
     *───────────────────────────────*/
    [Header("구르기 설정")]
    [SerializeField] float rollDistance = 4f;
    [SerializeField] float rollDuration = 0.4f;
    [SerializeField] float backstepDistance = 2f;
    [SerializeField] float backstepDuration = 0.25f;

    bool isRolling;
    Vector3 rollDirection;

    /*───────────────────────────────*
     * Animator Hash
     *───────────────────────────────*/
    int hashMoveX;
    int hashMoveY;
    int hashBlock;
    int hashRolling;
    int hashBackstep;

    enum RollType
    {
        Roll,
        Backstep
    }

    void Awake()
    {
        // 컴포넌트 참조
        anim = GetComponentInChildren<Animator>();
        cc = GetComponent<CharacterController>();
        pCombat = GetComponentInChildren<PlayerCombat>();
        core = GetComponent<PlayerCore>();

        // Animator Hash 초기화
        hashMoveX = Animator.StringToHash("moveX");
        hashMoveY = Animator.StringToHash("moveY");
        hashBlock = Animator.StringToHash("Block");
        hashRolling = Animator.StringToHash("Rolling");
        hashBackstep = Animator.StringToHash("Backstep");
    }

    //void OnEnable()
    //{
    //    InputManager.OnAttack += HandleAttack;
    //    InputManager.OnRolling += HandleRolling;
    //}

    //void OnDisable()
    //{
    //    InputManager.OnAttack -= HandleAttack;
    //    InputManager.OnRolling -= HandleRolling;
    //}

    void Update()
    {
        PlayerMove(InputManager.Input, InputManager.IsSprint);
        Block(InputManager.IsBlock);
        ApplyGravity();
    }

    /*───────────────────────────────*
     * 중력 처리
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
     * 이동 처리
     *───────────────────────────────*/

    /// <summary>
    /// 플레이어 이동 처리
    /// </summary>
    void PlayerMove(Vector2 input, bool isSprint)
    {
        // 공격 중이거나 구르기 중이면 이동 차단
        if ((pCombat != null && pCombat.isAttacking) || isRolling)
        {
            anim.SetFloat(hashMoveX, 0f);
            anim.SetFloat(hashMoveY, 0f);
            return;
        }

        if (input.magnitude < 0.1f)
        {
            anim.SetFloat(hashMoveX, 0f);
            anim.SetFloat(hashMoveY, 0f);
            return;
        }

        // 카메라 기준 이동 방향 계산
        Vector3 camForward = cameraRoot.forward;
        Vector3 camRight = cameraRoot.right;
        camForward.y = 0f;
        camRight.y = 0f;

        Vector3 moveDir = (camForward * input.y + camRight * input.x).normalized;

        // 기본 이동 속도
        float speed = core.WalkSpeed;

        // 가드 중에는 달리기 불가
        if (!InputManager.IsBlock && isSprint)
        {
            if (core.TryConsumeRunStamina(Time.deltaTime))
                speed = core.RunSpeed;
        }

        cc.Move(moveDir * speed * Time.deltaTime);

        // 비 락온 회전
        if (!IsLockOn)
        {
            Quaternion rot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
        }

        // 애니메이션 파라미터
        float animMul = isSprint ? 2f : 1f;

        if (!IsLockOn)
        {
            anim.SetFloat(hashMoveX, 0f);
            anim.SetFloat(hashMoveY, 1f * animMul);
        }
        else
        {
            Vector3 localDir = transform.InverseTransformDirection(moveDir);
            anim.SetFloat(hashMoveX, localDir.x);
            anim.SetFloat(hashMoveY, localDir.z * animMul);
        }
    }

    /*───────────────────────────────*
     * 전투 입력
     *───────────────────────────────*/

    void Block(bool isBlocked)
    {
        // 가드는 애니메이션 레이어/마스크로만 처리
        anim.SetBool(hashBlock, isBlocked);
    }

    void HandleAttack()
    {
        if (pCombat == null || isRolling)
            return;

        pCombat.OnCombo();
    }

    void HandleRolling()
    {
        if (isRolling)
            return;

        // 스태미나 부족 시 회피 불가
        if (!core.TryConsumeRollStamina())
            return;

        // 입력 여부에 따른 회피 타입 결정
        RollType rollType =
            InputManager.Input.magnitude < 0.1f
            ? RollType.Backstep
            : RollType.Roll;

        rollDirection = GetRollDirection(rollType);

        // 애니메이션 트리거
        if (rollType == RollType.Backstep)
            anim.SetTrigger(hashBackstep);
        else
            anim.SetTrigger(hashRolling);

        StartCoroutine(RollCoroutine(rollType));
    }

    /*───────────────────────────────*
     * 구르기 처리
     *───────────────────────────────*/

    /// <summary>
    /// 회피 방향 계산
    /// </summary>
    Vector3 GetRollDirection(RollType type)
    {
        if (type == RollType.Backstep)
        {
            // 백스텝은 항상 캐릭터 후방
            return -transform.forward;
        }

        Vector2 input = InputManager.Input;

        Vector3 camForward = cameraRoot.forward;
        Vector3 camRight = cameraRoot.right;
        camForward.y = 0f;
        camRight.y = 0f;

        return (camForward * input.y + camRight * input.x).normalized;
    }

    /// <summary>
    /// 회피 이동 처리
    /// </summary>
    IEnumerator RollCoroutine(RollType type)
    {
        isRolling = true;

        float duration = type == RollType.Backstep ? backstepDuration : rollDuration;
        float distance = type == RollType.Backstep ? backstepDistance : rollDistance;

        float elapsed = 0f;
        float speed = distance / duration;

        while (elapsed < duration)
        {
            cc.Move(rollDirection * speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isRolling = false;
    }
}
