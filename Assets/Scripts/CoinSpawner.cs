using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("코인 (빽빽한 장애물 사이에만)")]
    public GameObject coinPrefab;
    public int targetCount = 2;       // 동시에 유지할 코인 수 (아주 적게)
    public float minSpawnDist = 8f;   // 플레이어 코앞엔 안 생기게
    public float maxSpawnDist = 28f;  // 이 안의 장애물들 사이에 생성
    public float despawnDist = 42f;   // 이보다 멀면 제거
    public float minGap = 5f;         // 코인끼리 최소 간격
    public float clusterRadius = 9f;  // 두 장애물이 이 거리 안이면 '빽빽'으로 인정
    public float coinHeight = 1f;     // 띄우는 높이

    private Transform player;
    private readonly List<GameObject> coins = new List<GameObject>();
    private readonly List<Transform> nearbyObstacles = new List<Transform>();

    void Start()
    {
        PlayerController p = FindFirstObjectByType<PlayerController>();
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null || coinPrefab == null) return;

        // 멀거나 획득된(null) 코인 정리
        for (int i = coins.Count - 1; i >= 0; i--)
        {
            if (coins[i] == null) { coins.RemoveAt(i); continue; }
            if (Vector3.Distance(coins[i].transform.position, player.position) > despawnDist)
            {
                Destroy(coins[i]);
                coins.RemoveAt(i);
            }
        }

        GatherObstacles();
        if (nearbyObstacles.Count < 2) return; // 빽빽한 곳이 없으면 생성 안 함

        int attempts = 0;
        while (coins.Count < targetCount && attempts < 25)
        {
            attempts++;

            // 장애물 A와, 그 근처의 가장 가까운 장애물 B → 둘 사이 한가운데
            Transform a = nearbyObstacles[Random.Range(0, nearbyObstacles.Count)];
            Transform b = FindClusterPartner(a);
            if (b == null) continue; // A가 외딴 장애물이면 skip

            Vector3 mid = (a.position + b.position) * 0.5f;
            mid += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)); // 살짝 흔들기
            mid.y = coinHeight;

            if (Vector3.Distance(mid, player.position) < minSpawnDist) continue;

            bool tooClose = false;
            foreach (var c in coins)
                if (c != null && Vector3.Distance(c.transform.position, mid) < minGap) { tooClose = true; break; }
            if (tooClose) continue;

            GameObject coin = Instantiate(coinPrefab, mid, Quaternion.identity);
            Coin cc = coin.GetComponent<Coin>();
            if (cc != null) cc.player = player;
            coins.Add(coin);
        }
    }

    // A에서 clusterRadius 안의 가장 가까운 다른 장애물
    Transform FindClusterPartner(Transform a)
    {
        Transform best = null;
        float bestDist = clusterRadius;
        foreach (var ob in nearbyObstacles)
        {
            if (ob == a) continue;
            float d = Vector3.Distance(ob.position, a.position);
            if (d < bestDist) { bestDist = d; best = ob; }
        }
        return best;
    }

    void GatherObstacles()
    {
        nearbyObstacles.Clear();
        Collider[] cols = Physics.OverlapSphere(player.position, maxSpawnDist);
        foreach (var c in cols)
        {
            if (c.CompareTag("Obstacle")) nearbyObstacles.Add(c.transform);
        }
    }
}
