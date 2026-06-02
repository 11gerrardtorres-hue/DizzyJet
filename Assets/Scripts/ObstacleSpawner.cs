using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("장애물")]
    public GameObject obstaclePrefab;
    public int targetCount = 16;      // 플레이어 주변에 유지할 장애물 수
    public float minSpawnDist = 20f;  // 시야 밖에서 생성 (최소 거리)
    public float maxSpawnDist = 34f;  // 생성 최대 거리
    public float despawnDist = 44f;   // 이보다 멀어지면 제거
    public float minGap = 5f;         // 장애물끼리 최소 간격
    public float hitDist = 1.7f;      // 플레이어와 이 거리 안이면 격추

    [Header("카메라 자동 맞춤")]
    public bool autoFitToCamera = true; // 카메라 높이/화면비에 맞춰 거리·수 자동 계산
    public float densityFactor = 0.025f;// 시야 크기 대비 장애물 밀도

    private Transform player;
    private readonly List<GameObject> obstacles = new List<GameObject>();

    void Start()
    {
        PlayerController p = FindFirstObjectByType<PlayerController>();
        if (p != null) player = p.transform;

        if (autoFitToCamera) FitToCamera();
    }

    // 카메라가 내려다보는 시야 크기를 계산해 생성 거리·밀도를 자동 설정
    void FitToCamera()
    {
        Camera cam = Camera.main;
        float h = 42f, fov = 60f, aspect = 0.5f;
        if (cam != null)
        {
            CameraFollow follow = cam.GetComponent<CameraFollow>();
            h = (follow != null) ? follow.height : cam.transform.position.y;
            fov = cam.fieldOfView;
            aspect = cam.aspect;
        }

        // 바닥에서 보이는 반경 (코너까지)
        float vHalf = h * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float hHalf = vHalf * aspect;
        float visibleRadius = Mathf.Sqrt(vHalf * vHalf + hHalf * hHalf);

        minSpawnDist = visibleRadius + 4f;   // 화면 가장자리 바로 밖
        maxSpawnDist = visibleRadius + 18f;
        despawnDist  = visibleRadius + 30f;
        targetCount  = Mathf.RoundToInt(densityFactor * visibleRadius * visibleRadius);
    }

    void Update()
    {
        if (player == null || obstaclePrefab == null) return;

        CleanupFar();
        SpawnUpToTarget();
        CheckHit();
    }

    void CleanupFar()
    {
        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            if (obstacles[i] == null) { obstacles.RemoveAt(i); continue; }
            if (Vector3.Distance(obstacles[i].transform.position, player.position) > despawnDist)
            {
                Destroy(obstacles[i]);
                obstacles.RemoveAt(i);
            }
        }
    }

    void SpawnUpToTarget()
    {
        int attempts = 0;
        while (obstacles.Count < targetCount && attempts < 15)
        {
            attempts++;

            float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float d = Random.Range(minSpawnDist, maxSpawnDist);
            Vector3 pos = player.position + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * d;
            pos.y = 0f;

            // 기존 장애물과 너무 가까우면 다시 시도
            bool tooClose = false;
            foreach (var o in obstacles)
            {
                if (o != null && Vector3.Distance(o.transform.position, pos) < minGap) { tooClose = true; break; }
            }
            if (tooClose) continue;

            GameObject ob = Instantiate(obstaclePrefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            obstacles.Add(ob);
        }
    }

    void CheckHit()
    {
        foreach (var o in obstacles)
        {
            if (o != null && Vector3.Distance(o.transform.position, player.position) < hitDist)
            {
                if (GameManager.Instance != null) GameManager.Instance.GameOver();
                break;
            }
        }
    }
}
