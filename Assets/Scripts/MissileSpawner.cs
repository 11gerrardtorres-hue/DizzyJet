using UnityEngine;

public class MissileSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject missilePrefab;   // 복제할 미사일 프리팹
    public float spawnInterval = 3.5f; // 스폰 간격 (초)
    public float spawnDistance = 22f;  // 플레이어로부터 떨어진 거리
    public float sideSpread = 55f;     // 위/아래 기준 좌우로 퍼지는 각도

    private Transform player;
    private float timer;

    void Start()
    {
        PlayerController p = FindFirstObjectByType<PlayerController>();
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
        // 세로 화면은 위/아래가 잘 보임 → 주로 위(+Z) 또는 아래(-Z)에서 생성
        // 90도 = 위쪽, 270도 = 아래쪽. 거기서 좌우로 sideSpread만큼만 퍼뜨림
        float baseAngle = (Random.value < 0.5f) ? 90f : 270f;
        float angleDeg = baseAngle + Random.Range(-sideSpread, sideSpread);
        float angle = angleDeg * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnDistance;
        Vector3 spawnPos = player.position + offset;
        spawnPos.y = 0f;

        Instantiate(missilePrefab, spawnPos, Quaternion.identity);
    }
}