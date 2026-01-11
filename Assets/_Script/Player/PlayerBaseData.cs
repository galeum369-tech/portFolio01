using UnityEngine;

[CreateAssetMenu(menuName = "Data/Player Stats")]
public class PlayerBaseData : ScriptableObject
{
    [Header("체력/스테미나")]
    public int maxHP;
    public int maxStamina;

    [Header("공격 스탯")]
    public int STR;   // 근력
    public int DEX;   // 기량

    [Header("방어")]
    public int DEF;

    [Header("강인도")]
    public int Poise;

    [Header("유틸")]
    [Tooltip("스테미나 회복속도")]
    public float staminaRegen;
    [Tooltip("스테미나 회복 딜레이")]
    public float exhaustDelay = 1.5f;
    public float moveSpeed;
    public float runSpeed;

    [Header("스테미나 소모량")]
    [Tooltip("달리기")]
    public int runCostPerSecond;
    [Tooltip("구르기")]
    public int rollingCost;
}
