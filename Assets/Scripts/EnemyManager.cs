using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("적기 증원")]
    public GameObject enemyPrefab;    // 복제할 적기 프리팹
    public int maxEnemies = 3;        // 최대 적기 수
    public float rampInterval = 25f;  // 이 시간(초)마다 1기 증원
    public float spawnDistance = 20f; // 플레이어로부터 떨어진 거리
    public float sideSpread = 50f;    // 위/아래 기준 좌우로 퍼지는 각도(좁은 옆면 회피)

    private int spawned = 1;           // 씬에 이미 1기 있으므로 1부터 시작
    private Transform player;

    void Start()
    {
        PlayerController p = FindFirstObjectByType<PlayerController>();
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null || enemyPrefab == null) return;

        // 경과 시간에 따른 목표 적기 수 (1 + 단계), 최대치 제한
        int target = 1 + Mathf.FloorToInt(Time.timeSinceLevelLoad / rampInterval);
        target = Mathf.Min(target, maxEnemies);

        while (spawned < target)
        {
            SpawnEnemy();
            spawned++;
        }
    }

    void SpawnEnemy()
    {
        // 세로 화면은 위/아래가 잘 보임 → 위(+Z) 또는 아래(-Z)에서만 등장 (좌우 옆면 회피)
        float baseAngle = (Random.value < 0.5f) ? 90f : 270f;
        float angleDeg = baseAngle + Random.Range(-sideSpread, sideSpread);
        float ang = angleDeg * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * spawnDistance;
        Vector3 pos = player.position + offset;
        pos.y = 0f;
        Instantiate(enemyPrefab, pos, Quaternion.identity);
    }
}
