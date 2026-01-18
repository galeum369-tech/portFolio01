using UnityEngine;


/// <summary>
/// 플레이어 공격 히트박스
/// - WeaponBase에서 데미지/포이즈 값 참조
/// - 충돌 감지만 담당
/// - 실제 반응(FSM 전이)은 몬스터 FSM이 처리
/// </summary>
public class WeaponHitBox : MonoBehaviour
{
    WeaponBase weapon;

    void Awake()
    {
        // 기존 구조 유지
        weapon = GetComponentInParent<WeaponBase>();

        if (weapon == null)
        {
            Debug.LogError("[PlayerAttackHitBox] WeaponBase를 찾지 못했습니다.", this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (weapon == null)
            return;

        // 몬스터 FSM 기준으로 처리 (가장 단순 & 안정적)
        MonsterFSM_V2 monster = other.GetComponentInParent<MonsterFSM_V2>();
        if (monster == null)
            return;

        monster.TakeDamage(
            weapon.CalculateDamage(),
            weapon.GetPoiseDamage()
        );
    }
}

