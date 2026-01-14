using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator anim;
    [SerializeField] PlayerWeaponState weaponState;
    PlayerController PC;

    // 상태
    public bool isAttacking = false;
    bool isComboReserved = false;
    bool isInputWindowOpen = false;


    float stuckTimer = 0;
    public float stuckTimeOut = 5f;

    int attackHash;

    void Awake()
    {
        attackHash = Animator.StringToHash("Attack");
    }


    private void Update()
    {
        if (isAttacking == false)
        {
            // 공격 상태가 아니면 타이머 초기화
            stuckTimer = 0f;
            return;
        }

        // 공격 중인데 정상 종료 이벤트가 안 오고 있다
        stuckTimer += Time.deltaTime;

        if (stuckTimer >= stuckTimeOut)
        {
            ForceEndCombo();
        }
    }
    void ForceEndCombo()
    {
        isAttacking = false;
        isInputWindowOpen = false;
        isComboReserved = false;
        stuckTimer = 0f;


        anim.ResetTrigger(attackHash);
    }


    /*───────────────────────────────*
     * 입력 처리
     *───────────────────────────────*/
    public void OnCombo()
    {
        if (weaponState.isCombo == false)
        {
            anim.SetTrigger(attackHash);
            return;
        }
        if (isAttacking == false)
        {
            StartCombo();
        }
        else if (isInputWindowOpen)
        {
            // 입력 창이 열려있을 때 버튼을 누르면 다음 공격 예약
            isComboReserved = true;
        }
    }

    void StartCombo()
    {
        isAttacking = true;
        isComboReserved = false;
        anim.SetTrigger(attackHash);
    }

    /*───────────────────────────────*
     * Animation Events
     *───────────────────────────────*/

    // 🔔 입력 창 열기 (애니메이션 중간쯤)
    public void ComboInputStart()
    {
        isInputWindowOpen = true;
        isComboReserved = false; 
    }

    // 🔔 실제 다음 공격으로 넘어갈지 결정하는 시점 
    // (애니메이션 끝부분이 아니라, '연결'이 자연스러운 지점에 배치)
    public void ComboCheck()
    {
        isInputWindowOpen = false;

        if (isComboReserved)
        {
            isComboReserved = false;
            // 공격 트리거를 다시 작동
            anim.SetTrigger(attackHash);
        }
    }

    // 🔔 전체 공격 상태 종료 (애니메이션이 완전히 끝날 때쯤)
    public void EndCombo()
    {
        isAttacking = false;
        isComboReserved = false;
        isInputWindowOpen = false;
        
        // Idle로 돌아갈 때 남아있을지 모르는 트리거 청소
        anim.ResetTrigger(attackHash);
    }

    // Animation Event
    public void AttackStart()
    {
        var weapon = weaponState.CurrentWeapon;
        if (weapon == null) return;

        weapon.StartAttack();
    }

    // Animation Event
    public void AttackEnd()
    {
        var weapon = weaponState.CurrentWeapon;
        if (weapon == null) return;

        weapon.EndAttack();
    }

    public void ShootProjectile()
    {

    }

    public void ShootChargeProjectile()
    {

    }


    //애니메이션 이벤트 - 프로젝타일 생성
    public void FireProjectile(GameObject projectilePrefab, Transform firePoint)
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );
    }

    /*───────────────────────────────*
     * 애니메이션 이벤트 - 히트박스 온 오프
     *───────────────────────────────*/

    public void AttackStart(GameObject hitBox)
    {
        if (hitBox != null)
            hitBox.SetActive(true);
    }

    public void AttackEnd(GameObject hitBox)
    {
        if (hitBox != null)
            hitBox.SetActive(false);
    }
}

