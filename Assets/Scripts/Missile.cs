using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("미사일 설정")]
    public float speed = 25f;        // 전진 속도 (빠름 → 긴장감)
    public float turnSpeed = 110f;   // 선회 속도 (플레이어 일반선회 140보다 낮음 → 크게 돌면 못 따라옴)
    public float lifetime = 6f;      // 수명 (초)
    public float hitRadius = 1f;     // 플레이어와 이 거리 안이면 명중
    public float lockBreakAngle = 60f; // 이 각도 이상 벌어지면 락 해제(직진으로 빠짐)
    public float obstacleHitRadius = 0.6f; // 장애물과 이 거리 안이면 터짐

    private Transform target;
    private bool lockBroken;          // 급선회로 따돌려져 락이 풀렸는지

    void Start()
    {
        // 씬에서 플레이어를 자동으로 찾음
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            target = player.transform;

            // 스폰 순간 플레이어를 정조준 → 처음엔 직진으로 날아옴
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        // 수명이 끝나면 자동 소멸
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 장애물에 막히면 터짐 (엄폐 가능)
        if (HitObstacle())
        {
            Destroy(gameObject);
            return;
        }

        // 락이 살아있을 때만 추적 (락 풀리면 그냥 직진해서 화면 밖으로)
        if (target != null && !lockBroken)
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;

            // 미사일 진행 방향과 플레이어 방향 사이 각도
            float angle = Vector3.Angle(transform.forward, dir);

            if (angle > lockBreakAngle)
            {
                // 급선회로 따돌려짐 → 락 해제, 잠깐 직진하다 빨리 소멸
                lockBroken = true;
                Destroy(gameObject, 1.5f);
            }
            else
            {
                // 락 유지: 제한된 선회율로 추적
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, turnSpeed * Time.deltaTime);

                // 명중 판정
                float dist = Vector3.Distance(transform.position, target.position);
                if (dist < hitRadius)
                {
                    if (GameManager.Instance != null) GameManager.Instance.GameOver();
                    Destroy(gameObject);
                    return;
                }
            }
        }

        // 바라보는 방향으로 전진
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    // 주변에 'Obstacle' 태그 콜라이더가 있으면 true
    bool HitObstacle()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, obstacleHitRadius);
        foreach (var c in cols)
        {
            if (c.CompareTag("Obstacle")) return true;
        }
        return false;
    }
}