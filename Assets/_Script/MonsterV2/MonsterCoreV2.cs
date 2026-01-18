using UnityEngine;

public class MonsterCoreV2 : MonoBehaviour
{
    [SerializeField] MonsterBaseData data;

    public float CurrentHP { get; private set; }
    public float CurrentPoise { get; private set; }

    public bool IsDead => CurrentHP <= 0f;

    void Awake()
    {
        CurrentHP = data.maxHP;
        CurrentPoise = data.maxPoise;
    }

    public void TakeDamage(float damage)
    {
        float final = Mathf.Max(1f, damage - data.defense);
        CurrentHP -= final;

        if (CurrentHP < 0f)
            CurrentHP = 0f;
    }

    public bool ApplyPoiseDamage(float value)
    {
        CurrentPoise -= value;

        if (CurrentPoise <= 0f)
        {
            CurrentPoise = data.maxPoise;
            return true;
        }
        return false;
    }
}

