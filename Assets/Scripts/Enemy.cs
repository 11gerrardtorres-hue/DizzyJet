using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("추격 설정")]
    public float moveSpeed = 12f;     // 플레이어(15)보다 느림 → 평소엔 거리 유지
    public float hitDistance = 2.5f;  // 이 거리 안이면 충돌=격추

    [Header("미사일 발사")]
    public GameObject missilePrefab;
    public float fireInterval = 3f;   // 발사 간격 (초)

    private Transform player;
    private float fireTimer;

    void Start()
    {
        PlayerController p = FindObjectOfType<PlayerController>();
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

        transform.position += dir.normalized * moveSpeed * Time.deltaTime;
    }

    void CheckCollision()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < hitDistance)
        {
            Debug.Log("게임오버! 적기에 격추됨");
        }
    }

    void HandleFiring()
    {
        if (missilePrefab == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Instantiate(missilePrefab, transform.position, Quaternion.identity);
            fireTimer = fireInterval;
        }
    }
}