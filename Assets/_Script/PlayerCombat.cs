using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator anim;
    [SerializeField] PlayerWeaponState weaponState;
    PlayerController PC;

    // 상태
    public bool isAttacking = false;
    int comboInputCount = 0;
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


        if (comboInputCount >0) print(comboInputCount);
    }
    void ForceEndCombo()
    {
        isAttacking = false;
        isInputWindowOpen = false;
        comboInputCount = 0;
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
        else if (isInputWindowOpen == true)
        {
            comboInputCount++;
        }
    }

    void StartCombo()
    {
        isAttacking = true;
        anim.SetTrigger(attackHash);
        print("첫번째 공격");
    }

    /*───────────────────────────────*
     * Animation Events - Combo
     *───────────────────────────────*/

    // 🔔 입력 받기 시작
    public void ComboInputStart()
    {
        isInputWindowOpen = true;
        comboInputCount = 0;
    }



    // 🔔 입력 받기 종료
    public void ComboInputEnd()
    {
        isInputWindowOpen = false;

        if (comboInputCount > 0)
        {
            comboInputCount = 0;
            anim.SetTrigger(attackHash);
        }
        else
        {
            EndCombo();
        }
    }


    void EndCombo()
    {
        isAttacking = false;
        stuckTimer = 0f;
    }

    /*───────────────────────────────*
     * Animation Events - HitBox
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

