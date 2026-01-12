using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float speed = 15f;
    public float lifeTime = 3f;
    public int damage = 30;
    public int poiseDamage = 10;

    float timer;

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out MonsterCore monster))
            return;

        monster.TakeDamage(damage);

        if (monster.ApplyPoiseDamage(poiseDamage))
        {
            // 몬스터 경직은 FSM에서 처리
        }

        Destroy(gameObject);
    }
}
