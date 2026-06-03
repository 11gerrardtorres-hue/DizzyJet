using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("코인")]
    public GameObject coinPrefab;
    public int targetCount = 10;      // 주변에 유지할 코인 수
    public float minSpawnDist = 8f;   // 생성 최소 거리
    public float maxSpawnDist = 25f;  // 생성 최대 거리
    public float despawnDist = 40f;   // 이보다 멀면 제거
    public float minGap = 4f;         // 코인끼리 최소 간격
    public float coinHeight = 1f;     // 바닥에서 띄우는 높이

    private Transform player;
    private readonly List<GameObject> coins = new List<GameObject>();

    void Start()
    {
        PlayerController p = FindFirstObjectByType<PlayerController>();
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null || coinPrefab == null) return;

        // 멀어졌거나 획득된(null) 코인 정리
        for (int i = coins.Count - 1; i >= 0; i--)
        {
            if (coins[i] == null) { coins.RemoveAt(i); continue; }
            if (Vector3.Distance(coins[i].transform.position, player.position) > despawnDist)
            {
                Destroy(coins[i]);
                coins.RemoveAt(i);
            }
        }

        // 부족하면 생성
        int attempts = 0;
        while (coins.Count < targetCount && attempts < 15)
        {
            attempts++;

            float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(minSpawnDist, maxSpawnDist);
            Vector3 pos = player.position + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * dist;
            pos.y = coinHeight;

            bool tooClose = false;
            foreach (var c in coins)
                if (c != null && Vector3.Distance(c.transform.position, pos) < minGap) { tooClose = true; break; }
            if (tooClose) continue;

            GameObject coin = Instantiate(coinPrefab, pos, Quaternion.identity);
            Coin cc = coin.GetComponent<Coin>();
            if (cc != null) cc.player = player;
            coins.Add(coin);
        }
    }
}
