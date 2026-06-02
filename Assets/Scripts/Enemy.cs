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

    [Header("미사일 발사")]
    public GameObject missilePrefab;
    public float fireInterval = 3f;   // 발사 간격 (초)

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
        // 플레이어 방향으로 회전 + 전진
        Vector3 dir = player.position - transform.position;
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
            currentMissile = Instantiate(missilePrefab, transform.position, Quaternion.identity);
            fireTimer = fireInterval;
        }
    }
}