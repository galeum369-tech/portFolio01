using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MonsterHPBar
/// 
/// [역할]
/// - 몬스터 체력을 월드 공간에서 표시
/// - 카메라를 항상 바라봄
/// 
/// [사용 위치]
/// - 일반 몬스터 프리팹
/// - Canvas (World Space) 하위
/// </summary>
public class MonsterHPBar : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] MonsterCore core;
    [SerializeField] Image fillImage;

    [Header("옵션")]
    [SerializeField] Vector3 offset = new Vector3(0f, 2.2f, 0f);

    Camera mainCam;
    float maxHP;

    void Awake()
    {
        if (core == null)
            core = GetComponentInParent<MonsterCore>();

        mainCam = Camera.main;

        maxHP = core.BaseData.maxHP;
    }

    void LateUpdate()
    {
        if (core == null || mainCam == null)
            return;

        // 위치 보정
        transform.position = core.transform.position + offset;

        // 카메라 응시
        transform.forward = mainCam.transform.forward;

        // 체력 반영
        fillImage.fillAmount = core.CurrentHP / maxHP;
    }
}

