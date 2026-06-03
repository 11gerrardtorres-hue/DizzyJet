using System.Collections.Generic;
using UnityEngine;

// 드리프트 중에 위협(미사일/적기/장애물)이 가까이 스쳐 지나가면 니어미스로 카운트
public class NearMissDetector : MonoBehaviour
{
    public float nearMissRadius = 3.5f;  // 이 반경에 위협이 새로 들어오면 니어미스

    private PlayerController player;
    private readonly HashSet<int> inZone = new HashSet<int>();
    private readonly HashSet<int> seenThisFrame = new HashSet<int>();

    void Start()
    {
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        seenThisFrame.Clear();

        Collider[] cols = Physics.OverlapSphere(transform.position, nearMissRadius);
        bool drifting = player != null && player.IsDrifting;

        foreach (var c in cols)
        {
            if (!IsThreat(c)) continue;
            int id = c.GetInstanceID();
            seenThisFrame.Add(id);

            // 위협이 '새로' 근접 반경에 들어왔고, 드리프트 중이면 니어미스
            if (!inZone.Contains(id) && drifting && GameManager.Instance != null)
                GameManager.Instance.RegisterNearMiss();
        }

        // 다음 프레임 비교를 위해 갱신
        inZone.Clear();
        foreach (var id in seenThisFrame) inZone.Add(id);
    }

    bool IsThreat(Collider c)
    {
        // 미사일만 니어미스로 인정
        return c.GetComponent<Missile>() != null;
    }
}
