using UnityEngine;

/// <summary>
/// PlayerCombatV2
/// 
/// [역할]
/// - 공격 실행 및 콤보 관리
/// - 애니메이션 이벤트 수신
/// - 무기 / 프로젝타일 트리거
/// 
/// ⚠ 입력 판단 ❌
/// ⚠ 차지 시간 계산 ❌
/// </summary>
public class PlayerCombatV2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator anim;
    [SerializeField] PlayerWeaponStateV2 weaponState;
    /*───────────────────────────────*
     * 상태
     *───────────────────────────────*/
    public bool IsInAction => isAttacking;

    bool isAttacking;
    bool isComboReserved;
    bool isInputWindowOpen;

    float stuckTimer;
    [SerializeField] float stuckTimeout = 5f;

    int hashAttack;
    int hashCharge;

    void Awake()
    {
        hashAttack = Animator.StringToHash("Attack");
        hashCharge = Animator.StringToHash("Charge");
    }

    void Update()
    {
        if (!isAttacking)
        {
            stuckTimer = 0f;
            return;
        }

        stuckTimer += Time.deltaTime;
        if (stuckTimer >= stuckTimeout)
        {
            Debug.Log("[CombatV2] 공격 종료 이벤트 누락 → 강제 종료");
            ForceEndCombo();
        }
    }

    /*───────────────────────────────*
     * 내부 처리
     *───────────────────────────────*/
    void ForceEndCombo()
    {
        isAttacking = false;
        isComboReserved = false;
        isInputWindowOpen = false;
        stuckTimer = 0f;

        anim.ResetTrigger(hashAttack);
    }

    /*───────────────────────────────*
     * 외부 호출 API (Controller 전용)
     *───────────────────────────────*/

    /// <summary>
    /// 일반 공격 실행
    /// </summary>
    public void ExecuteNormalAttack()
    {
        // 🔹 콤보 불가 무기 (마법 등)
        if (!weaponState.IsComboWeapon)
        {
            Debug.Log("[CombatV2] 단타 공격 실행 (콤보 불가 무기)");

            ForceEndCombo();
            isAttacking = true;
            anim.SetTrigger(hashAttack);
            return;
        }

        // 🔹 콤보 무기
        if (!isAttacking)
        {
            StartCombo();
        }
        else if (isInputWindowOpen)
        {
            isComboReserved = true;
        }
    }

    /// <summary>
    /// 차지 공격 실행
    /// </summary>
    public void ExecuteChargeAttack()
    {
        Debug.Log("[CombatV2] 차지 공격 실행");

        // 차지는 항상 단일 액션
        ForceEndCombo();
        isAttacking = true;

        anim.SetTrigger(hashCharge);
    }

    void StartCombo()
    {
        Debug.Log("[CombatV2] 콤보 시작");

        isAttacking = true;
        isComboReserved = false;
        anim.SetTrigger(hashAttack);
    }

    /*───────────────────────────────*
     * Animation Events
     *───────────────────────────────*/

    /// <summary>
    /// 콤보 입력 허용 구간 시작
    /// </summary>
    public void ComboInputStart()
    {
        if (!weaponState.IsComboWeapon)
            return;

        isInputWindowOpen = true;
        isComboReserved = false;
    }

    /// <summary>
    /// 다음 콤보로 이어질지 판단
    /// </summary>
    public void ComboCheck()
    {
        if (!weaponState.IsComboWeapon)
            return;

        isInputWindowOpen = false;

        if (isComboReserved)
        {
            Debug.Log("[CombatV2] 콤보 연결");
            isComboReserved = false;
            anim.SetTrigger(hashAttack);
        }
    }

    /// <summary>
    /// 공격 전체 종료
    /// </summary>
    public void EndCombo()
    {
        Debug.Log("[CombatV2] 공격 종료");

        isAttacking = false;
        isComboReserved = false;
        isInputWindowOpen = false;

        anim.ResetTrigger(hashAttack);
    }

    /*───────────────────────────────*
     * 히트박스 (근접 무기)
     *───────────────────────────────*/

    public void AttackStart()
    {
        if (weaponState.CurrentWeapon == null)
            return;

        weaponState.CurrentWeapon.StartAttack();
    }

    public void AttackEnd()
    {
        if (weaponState.CurrentWeapon == null)
            return;

        weaponState.CurrentWeapon.EndAttack();
    }

    /*───────────────────────────────*
     * Projectile (마법 전용)
     *───────────────────────────────*/

    public void ShootProjectile()
    {
        Debug.Log("[CombatV2] 기본 마법 프로젝타일 발사");
        // 생성 로직은 이후 Projectile 스크립트에서
    }

    public void ShootChargeProjectile()
    {
        Debug.Log("[CombatV2] 차지 마법 프로젝타일 발사");
    }
}

