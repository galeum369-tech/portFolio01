using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float speed = 15f;
    public float lifeTime = 3f;
    public int damage = 30;
    public int poiseDamage = 10;

    [Header("유도 설정")]
    [Tooltip("락온 타겟 (없으면 직진)")]
    public Transform target;

    [Tooltip("회전 보정 속도")]
    public float turnSpeed = 15f;

    float timer;

    void Update()
    {
        MoveProjectile();
        HandleLifeTime();
    }

    void MoveProjectile()
    {
        // 🔹 락온 타겟이 살아있다면 추적
        if (target != null)
        {
            Vector3 dir = target.position - transform.position;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    Time.deltaTime * turnSpeed
                );
            }
        }

        // 전진
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void HandleLifeTime()
    {
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
            // 경직/스태거는 FSM에서 처리
        }

        Destroy(gameObject);
    }
}

