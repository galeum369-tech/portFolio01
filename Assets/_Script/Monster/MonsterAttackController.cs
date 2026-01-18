using UnityEngine;

/// <summary>
/// MonsterAttackController
///
/// [역할]
/// - 공격 애니메이션 이벤트를 통해
///   공격 히트박스 GameObject를 ON / OFF
///
/// [특징]
/// - 히트박스 내부 로직에는 관여 ❌
/// - FSM / 데미지 계산과 완전 분리
/// - 애니메이터 전용 중계 스크립트
/// </summary>
public class MonsterAttackController : MonoBehaviour
{
    [Header("공격 히트박스 오브젝트")]
    [Tooltip("공격 시 활성화할 히트박스 GameObject들")]
    [SerializeField] GameObject[] attackHitBoxes;

    /// <summary>
    /// 공격 판정 시작 (애니메이션 이벤트)
    /// </summary>
    public void AttackHitStart()
    {
        foreach (var box in attackHitBoxes)
        {
            if (box != null)
                box.SetActive(true);
        }
    }

    /// <summary>
    /// 공격 판정 종료 (애니메이션 이벤트)
    /// </summary>
    public void AttackHitEnd()
    {
        foreach (var box in attackHitBoxes)
        {
            if (box != null)
                box.SetActive(false);
        }
    }

    /// <summary>
    /// 안전장치: 비활성화 시 히트박스 강제 OFF
    /// (사망 / 상태 전이 중 잔존 방지)
    /// </summary>
    void OnDisable()
    {
        AttackHitEnd();
    }
}
