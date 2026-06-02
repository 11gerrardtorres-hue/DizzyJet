using UnityEngine;

public class MissileSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject missilePrefab;   // 복제할 미사일 프리팹
    public float spawnInterval = 3.5f; // 스폰 간격 (초)
    public float spawnDistance = 30f;  // 플레이어로부터 떨어진 거리

    private Transform player;
    private float timer;

    void Start()
    {
        PlayerController p = FindObjectOfType<PlayerController>();
        if (p != null) player = p.transform;
        timer = spawnInterval;
    }

    void Update()
    {
        if (player == null || missilePrefab == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnMissile();
            timer = spawnInterval;
        }
    }

    void SpawnMissile()
    {
        // 플레이어 주변 랜덤 방향에서 미사일 생성
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnDistance;
        Vector3 spawnPos = player.position + offset;
        spawnPos.y = 0f;

        Instantiate(missilePrefab, spawnPos, Quaternion.identity);
    }
}