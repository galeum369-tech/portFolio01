using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public PlayerStatData baseData;

    public float CurrentHP { get; private set; }
    public float CurrentStamina { get; private set; }

    void Awake()
    {
        CurrentHP = baseData.maxHP;
        CurrentStamina = baseData.maxStamina;
    }
}

