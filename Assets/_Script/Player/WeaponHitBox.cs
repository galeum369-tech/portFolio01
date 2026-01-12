using UnityEngine;

/// <summary>
/// 무기 공격용 히트박스
/// - 충돌 감지만 담당
/// - 공격 단위 중복 피격 방지
/// - 실제 판단은 MonsterCore에서 수행
/// </summary>
public class WeaponHitBox : MonoBehaviour
{
    WeaponBase weapon;

    void Awake()
    {
        // 부모에 WeaponBase가 반드시 하나 존재해야 함
        weapon = GetComponentInParent<WeaponBase>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (weapon == null)
            return;

        // 몬스터 코어 찾기 (HitCollider가 자식일 수도 있음)
        MonsterCore monster = other.GetComponentInParent<MonsterCore>();
        if (monster == null)
            return;

        // 이 공격에서 이미 맞았는지 체크
        if (!monster.CanBeHit(weapon.CurrentAttackId))
            return;

        // 데미지 적용
        monster.TakeDamage(weapon.CalculateDamage());

        // 강인도 처리
        if (monster.ApplyPoiseDamage(weapon.GetPoiseDamage()))
        {
            // 경직/스태거는 FSM에서 처리
        }
    }
}

