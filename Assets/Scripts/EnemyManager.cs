using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("적기 증원")]
    public GameObject enemyPrefab;    // 복제할 적기 프리팹
    public int maxEnemies = 3;        // 최대 적기 수
    public float rampInterval = 25f;  // 이 시간(초)마다 1기 증원
    public float spawnDistance = 28f; // 플레이어로부터 떨어진 거리
    public float formationSpacing = 4f; // 적기 간 좌우 간격
    public bool autoFitToCamera = true; // 화면 아래 가장자리 밖으로 자동 맞춤

    private int spawned = 1;           // 씬에 이미 1기 있으므로 1부터 시작
    private Transform player;

    void Start()
    {
        PlayerController p = FindFirstObjectByType<PlayerController>();
        if (p != null) player = p.transform;

        if (autoFitToCamera)
        {
            Camera cam = Camera.main;
            float h = 36f, fov = 60f;
            if (cam != null)
            {
                CameraFollow follow = cam.GetComponent<CameraFollow>();
                h = (follow != null) ? follow.height : cam.transform.position.y;
                fov = cam.fieldOfView;
            }
            // 화면 세로 절반 + 여유 → 아래 가장자리 바로 밖에서 등장
            float vHalf = h * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            spawnDistance = vHalf + 9f;
        }
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
        // 항상 화면 아래쪽(-Z)에서만 등장 → 뒤에서 쫓아오는 느낌 + 미리 보여서 대응 가능
        float angleDeg = 270f + Random.Range(-18f, 18f);
        float ang = angleDeg * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * spawnDistance;
        Vector3 pos = player.position + offset;
        pos.y = 0f;

        GameObject e = Instantiate(enemyPrefab, pos, Quaternion.identity);

        // 좌우 대형 오프셋 부여: +s, -s, +2s, -2s ... (씬 적기는 0)
        Enemy en = e.GetComponent<Enemy>();
        if (en != null)
        {
            float off = (spawned % 2 == 1)
                ? formationSpacing * ((spawned + 1) / 2)
                : -formationSpacing * (spawned / 2);
            en.formationOffset = off;
        }
    }
}
