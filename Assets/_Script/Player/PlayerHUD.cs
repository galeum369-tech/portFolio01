using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 상태 UI
/// - 체력 / 스태미나 표시
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("참조")]
    public PlayerCoreV2 core;

    [Header("UI")]
    public Image hpFill;
    public Image staminaFill;

    int maxHP;
    float maxStamina;

    void Start()
    {
        if (core == null) return;

        maxHP = core.GetComponent<PlayerCoreV2>().CurrentHP;
        maxStamina = core.GetComponent<PlayerCoreV2>().CurrentStamina;
    }

    void Update()
    {
        if (core == null) return;

        hpFill.fillAmount = (float)core.CurrentHP / maxHP;
        staminaFill.fillAmount = (float)core.CurrentStamina / maxStamina;
    }
}
