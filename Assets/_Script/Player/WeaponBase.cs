using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모든 무기의 런타임 베이스
/// - 공격 단위(AttackId) 관리
/// - 히트박스 제어
/// - 데미지 계산
/// 
/// 애니메이션 이벤트는 PlayerCombat에서 받아
/// 이 클래스를 호출한다.
/// </summary>
public class WeaponBase : MonoBehaviour
{
    [Header("무기 데이터")]
    [SerializeField] protected WeaponData weaponData;

    [Header("히트박스")]
    [Tooltip("이 무기가 사용하는 모든 히트박스")]
    [SerializeField] protected List<Collider> hitBoxes = new List<Collider>();

    protected PlayerCore playerCore;

    int currentAttackId = 0;
    public int CurrentAttackId => currentAttackId;

    protected virtual void Awake()
    {
        playerCore = GetComponentInParent<PlayerCore>();

        // 시작 시 히트박스 전부 비활성
        foreach (var col in hitBoxes)
        {
            col.enabled = false;
        }
    }

    /*───────────────────────────────*
     * 공격 제어 (Combat에서 호출)
     *───────────────────────────────*/

    /// <summary>
    /// 공격 시작 시 호출
    /// </summary>
    public void StartAttack()
    {
        currentAttackId++;

        foreach (var col in hitBoxes)
        {
            col.enabled = true;
        }
    }

    /// <summary>
    /// 공격 종료 시 호출
    /// </summary>
    public void EndAttack()
    {
        foreach (var col in hitBoxes)
        {
            col.enabled = false;
        }
    }

    /*───────────────────────────────*
     * 데미지 계산
     *───────────────────────────────*/

    public int CalculateDamage()
    {
        if (weaponData == null || playerCore == null)
            return 0;

        int damage =
            weaponData.baseDamage +
            Mathf.RoundToInt(playerCore.STR * weaponData.strScaling) +
            Mathf.RoundToInt(playerCore.DEX * weaponData.dexScaling);

        return Mathf.Max(1, damage);
    }

    public int GetPoiseDamage()
    {
        return weaponData != null ? weaponData.poiseDamage : 0;
    }
}

