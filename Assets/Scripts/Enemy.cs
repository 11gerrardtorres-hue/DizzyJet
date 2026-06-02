using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("추격 설정 (고무줄)")]
    public float preferredDistance = 13f; // 유지하려는 뒤 거리
    public float baseSpeed = 15f;         // 거리 유지 시 기준 속도(플레이어와 비슷)
    public float chaseGain = 1.2f;        // 거리 차이에 반응하는 정도
    public float minSpeed = 8f;           // 최저 속도 (이보다 느린 플레이어는 따라잡음)
    public float maxSpeed = 21f;          // 최고 속도 (멀어지면 잠깐 빨라져 따라붙음)
    public float hitDistance = 2.5f;      // 이 거리 안이면 충돌=격추
    public float formationOffset = 0f;    // 좌우 대형 오프셋 (적기들이 겹치지 않게 나란히)

    [Header("미사일 발사")]
    public GameObject missilePrefab;
    public float fireInterval = 3f;   // 발사 간격 (초)

    [Header("난이도 램프")]
    public float rampInterval = 25f;     // 이 시간(초)마다 난이도 1단계 상승
    public float fireIntervalMult = 0.9f;// 단계마다 발사 간격 ×0.9 (더 자주)
    public float missileSpeedMult = 1.04f;// 단계마다 미사일 속도 ×1.04 (더 빠름)
    public float minFireInterval = 0.6f; // 발사 간격 하한

    private Transform player;
    private float fireTimer;
    private GameObject currentMissile;  // 공중에 떠 있는 미사일 (한 번에 하나)

    void Start()
    {
        PlayerController p = FindFirstObjectByType<PlayerController>();
        if (p != null) player = p.transform;
        fireTimer = fireInterval;
    }

    void Update()
    {
        if (player == null) return;

        ChasePlayer();
        CheckCollision();
        HandleFiring();
    }

    void ChasePlayer()
    {
        // 추격 방향 기준 좌우로 오프셋을 준 '대형 위치'를 목표로 → 적기들이 나란히
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        Vector3 perp = Vector3.Cross(Vector3.up, toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : Vector3.forward);
        Vector3 targetPos = player.position + perp * formationOffset;

        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
        }

        // 고무줄 추격: 선호 거리보다 멀면 빨라지고, 가까우면 느려짐
        float dist = dir.magnitude;
        float error = dist - preferredDistance;
        float speed = Mathf.Clamp(baseSpeed + error * chaseGain, minSpeed, maxSpeed);

        transform.position += dir.normalized * speed * Time.deltaTime;
    }

    void CheckCollision()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < hitDistance)
        {
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
        }
    }

    void HandleFiring()
    {
        if (missilePrefab == null) return;

        // 이미 쏜 미사일이 아직 살아있으면 다음 발사 안 함 (한 번에 하나)
        if (currentMissile != null) return;

        // 미사일이 사라졌으면 재장전 타이머 진행 후 다음 발사
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            // 경과 시간으로 난이도 단계 계산
            int step = Mathf.FloorToInt(Time.timeSinceLevelLoad / rampInterval);

            currentMissile = Instantiate(missilePrefab, transform.position, Quaternion.identity);

            // 난이도: 미사일 속도 상승
            Missile mis = currentMissile.GetComponent<Missile>();
            if (mis != null) mis.speed *= Mathf.Pow(missileSpeedMult, step);

            // 난이도: 발사 간격 단축 (하한 적용)
            float interval = fireInterval * Mathf.Pow(fireIntervalMult, step);
            fireTimer = Mathf.Max(minFireInterval, interval);
        }
    }
}