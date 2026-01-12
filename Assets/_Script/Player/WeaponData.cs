using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponData",
    menuName = "Weapon/Weapon Data"
)]
public class WeaponData : ScriptableObject
{
    [Header("기본 정보")]
    public string weaponName;

    [Header("데미지")]
    public int baseDamage;
    public int poiseDamage;

    [Header("스탯 스케일링")]
    [Tooltip("STR 계수")]
    public float strScaling = 1f;

    [Tooltip("DEX 계수")]
    public float dexScaling = 0.5f;
}

